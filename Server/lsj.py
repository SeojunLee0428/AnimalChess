# -*- coding:utf-8 -*-
'''
Created on 2019. 2. 11.

@author: lee
'''

from flask import Flask, request, jsonify, session, redirect, url_for, escape
import pymysql, random
import os
from _overlapped import NULL
from time import strftime
from datetime import datetime

app = Flask(__name__)

dbInfo = {
    'host':'127.0.0.1',
    'port':3306,
    'user':'root',
    'password':'autoset',
    'db':'animalchess',
    'charset':'utf8'
}   # 데이터 베이스 정보

@app.route('/join', methods=['POST'])   # 회원 가입 요청
def join():
        id = request.json['id']
        passwd = request.json['passwd']
        nick = request.json['nick']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select count(*) as cnt from user where id=%s"
        curs.execute(sql, (id))
        dbid = curs.fetchone()[0]
 
        if dbid > 0:   # 회원가입실패
            result = {'url':'join', 'result':'false'}
        else:
            sql = "insert into user(id, passwd, nick) values(%s, password(%s), %s)"
            curs.execute(sql, (id, passwd, nick))
            conn.commit()
            sql = "insert into ranking(win, lose, rate) values(0, 0, 0)"
            curs.execute(sql)
            conn.commit()
            result = {'url':'join', 'result':'true'}
            
        conn.close()    
        return jsonify(result)

@app.route('/login', methods=['POST'])   # 로그인 요청
def login():
        id = request.json['id']
        passwd = request.json['passwd']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        
        sql = "select login.user_idx from login inner join user on login.user_idx = user.idx where user.id = %s" 
        curs.execute(sql, (id))
        log = curs.fetchone()

        if log != None:   # 접속중인 아이디
            result = {'url':'login', 'result':'logon'}
        else:
            sql = "select user.idx from user where id = %s and passwd = password(%s)"
            curs.execute(sql, (id, passwd))
            data = curs.fetchone()
            
            if data == None:   # 로그인 실패
                result = {'url':'login', 'result':'false'}
            else:
                sql = "select user.log, user.count from user where id = %s"
                curs.execute(sql,(id))
                log = curs.fetchall()

                for value in log:
                    date = str(value[0])
                    count = value[1]
                
                now = datetime.now()
                today = now.strftime('%Y-%m-%d')

                if date != today:   # 로그인 날짜 확인 후 카운트 초기화
                    sql = "update user set user.log = %s where id = %s"
                    curs.execute(sql,(today,id))
                    conn.commit()
                    
                    if count < 80:
                        sql = "update user set user.count = 80 where id = %s"
                        curs.execute(sql,(id))
                        conn.commit()
                
                idx = data[0]
                sql = "insert into login(user_idx) values(%s)"
                curs.execute(sql, (idx))
                conn.commit()
                session['idx'] = idx
                result = {'url':'login', 'result':'true'}
            
        conn.close()
        return jsonify(result)   

@app.route('/logout', methods=['POST'])   # 로그아웃 요청
def logout():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "delete from login where user_idx = %s"
        curs.execute(sql, (session['idx']))
        conn.commit()
        conn.close()
        
        session.clear()
        
        result = {'url':'logout', 'result':'logout'}
        return jsonify(result)
    
@app.route('/lobby/info', methods=['POST'])   # 로비 사용자 정보 요청
def lobbyInfo():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select user.nick, user.coin, user.count from user where idx = %s"
        curs.execute(sql, (session['idx']))
        data = curs.fetchall()

        for row in data:
            nick = row[0]
            coin = row[1]
            count = row[2]

        result = {'url':'lobby/info', 'nick':nick, 'coin':coin, 'count':count, 'result':'true'}
        return jsonify(result)

@app.route('/lobby/rank', methods=['POST'])   # 로비 랭킹 정보 요청
def lobbyRank():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select ranking.win,ranking.lose,ranking.rate from ranking where user_idx = %s"
        curs.execute(sql, (session['idx']))
        data = curs.fetchall()
        
        result = {}
        
        for row in data:
            result['win'] = row[0]
            result['lose'] = row[1]
            result['rate'] = row[2]

        if result['win'] == 0 and result['lose'] == 0:   # 새로운 유저 랭킹처리 제외
            result['rank'] = '--'
        else:
            sql = "select count(*)+1 as `rank` from ranking where (win != 0 or lose != 0) and score>(select score from ranking where user_idx = %s)"
            curs.execute(sql, (session['idx']))
            result['rank'] = curs.fetchone()[0]
            
        sql = "select u.nick from RANKING r left join user u on (u.idx = r.user_idx) where (win > 0) or (lose > 0) order by score desc limit 3"
        curs.execute(sql)
        data = curs.fetchall()
        conn.close()  
        
        for i in range(3):
            result['ranks'+str(i)] = '--'
        
        if data == ():   # 랭커 목록 불러오기 실패
            result['result'] = 'false'   
        else:
            rowCnt = 0
            
            for row in data:
                strCnt = str(rowCnt)
                result['ranks' + strCnt] = row[0]
                rowCnt += 1
                
            result['result'] = 'true'
             
        result['url'] = 'lobby/rank'

        return jsonify(result)

@app.route('/store', methods=['POST'])   # 상점 이용 요청
def Store():
        jsonItem = request.json['item']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        
        sql = "select user.coin,user.count from user where user.idx = %s"
        curs.execute(sql, (session['idx']))
        data = curs.fetchall()

        for row in data:
            coin = row[0]
            count = row[1]
        
        if jsonItem == "00" and int(coin) >= 200:   
            sql = "update user set count = count+1, coin = coin-200 where user.idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            count += 1
            coin -= 200
            result = {'url':'store', 'coin':coin, 'count':count, 'result':'true'}
        elif jsonItem == "01" and int(coin) >= 500:
            sql = "update user set count = count+5, coin = coin-500 where user.idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            count += 5
            coin -= 500
            result = {'url':'store', 'coin':coin, 'count':count, 'result':'true'}
        elif jsonItem == "02" and int(coin) >= 1000:
            sql = "update user set count = count+15, coin = coin-1000 where user.idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            count += 15
            coin -= 1000
            result = {'url':'store', 'coin':coin, 'count':count, 'result':'true'}
        else:   # 상점 구매 실패
            result = {'url':'store', 'result':'false'}
         
        conn.close()
        return jsonify(result)
    
@app.route('/users', methods=['POST'])   # 유저 목록 갱신 요청
def users():
        jsonUsers = request.json['users']

        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select user.nick, login.user_idx from user inner join login on user.idx=login.user_idx"
        curs.execute(sql)
        data = curs.fetchall()

        result = {}
        rowCnt = 0
        users = 0
        
        for row in data:
            strCnt = str(rowCnt)
            result['data' + strCnt] = row[0]
            rowCnt += 1
            users += int(row[1])
        
        users = str(users)
        
        if users != jsonUsers:
            result['users'] = users
            result['url'] = 'users'
            result['result'] = 'true'
        else:   # 유저 목록 갱신 실패
            result = {'url':'users', 'result':'false'}

        conn.close()
        return jsonify(result)  
          
@app.route('/allchat', methods=['POST'])   # 채팅 요청 
def allChat():
        jsonState = request.json['state']
        jsontime = request.json['time']
        jsonInput = request.json['input']
       
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        
        if jsonState == "push":
            sql = "select count(*) as cnt from chat"
            curs.execute(sql)
            cnt = curs.fetchone()[0]
            
            if cnt == 50:   # 채팅 최대치 초과시 삭제
                sql = "delete from chat ORDER BY time asc LIMIT 1"
                curs.execute(sql)
                conn.commit()
            
            sql = "insert into chat(login_idx,message,time) values(%s,%s,now())"
            curs.execute(sql, (session['idx'], jsonInput))
            conn.commit()
        
        sql = "SELECT date_format(time,'%T') FROM `chat` ORDER BY time DESC LIMIT 1"
        curs.execute(sql)
        time = curs.fetchone()[0]
       
        if time != jsontime:
            sql = "select user.nick,chat.message,date_format(time,'%T') from chat left join user on chat.login_idx = user.idx ORDER BY time asc"
            curs.execute(sql)
            data = curs.fetchall()
        
            result = {}
            rowCnt = 0
            
            for row in data:
                strCnt = str(rowCnt)
                result['nick' + strCnt] = row[0]
                result['message' + strCnt] = row[1]
                result['time' + strCnt] = row[2]
                rowCnt += 1
            
            result['url'] = 'allchat'
            result['result'] = 'true'
        
        else:   # 채팅 갱신 실패
            result = {'url':'allchat', 'result':'false'}
            
        conn.close()
        return jsonify(result)
 
@app.route('/rooms/poll', methods=['POST'])   # 방 목록 갱신 요청
def roomsPoll():
        jsonRoom = request.json['rooms']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select user.nick,room.host from room inner join user on room.host = user.idx where guest = '0'"    
        curs.execute(sql)
        data = curs.fetchall()
        conn.close()

        result = {}
        rowCnt = 0
        rooms = 0
        
        for row in data:
            strCnt = str(rowCnt)
            result['nick' + strCnt] = row[0]
            rowCnt += 1
            rooms += int(row[1])
            
        rooms = str(rooms)
        
        if rooms != jsonRoom: 
            result['rooms'] = rooms
            result['url'] = 'rooms/poll'
            result['result'] = 'true'
        else:   # 방 목록 갱신 실패
            result = {'url':'rooms/poll', 'result':'false'}

        return jsonify(result)
    
@app.route('/rooms/create', methods=['POST'])   # 방 생성
def roomsCreate():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "insert into room(host,guest) values(%s,0)"
        curs.execute(sql, (session['idx']))
        sql = "insert into play(login_idx,ready,state,count,`position`,attack) values(%s,false,0,3,0,0)"
        curs.execute(sql, (session['idx']))
        conn.commit()
        conn.close()

        result = {'url':'rooms/create', 'result':'true'}
        return jsonify(result)
        
@app.route('/rooms/match', methods=['POST'])   # 방 빠른 매칭 요청
def roomsMatch():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "update room set guest = %s where guest = 0 limit 1"    
        curs.execute(sql, (session['idx']))
        conn.commit()
        sql = "insert into play(login_idx,ready,state,count,`position`,attack) values(%s,false,0,3,0,0)"
        curs.execute(sql, (session['idx']))
        conn.commit()
        conn.close()
        
        result = {'url':'rooms/match', 'result':'true'}
        return jsonify(result)
         
@app.route('/rooms/select', methods=['POST'])   # 선택한 방 매칭 요청
def roomsSelect():
        jsonSelect = request.json['select']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "update room set guest = %s where host = (select user.idx from user where user.nick = %s) limit 1"    
        curs.execute(sql, (session['idx'], jsonSelect))
        conn.commit()
        sql = "insert into play(login_idx,ready,state,count,`position`,attack) values(%s,false,0,3,0,0)"
        curs.execute(sql, (session['idx']))
        conn.commit()
        conn.close()
        
        result = {'url':'rooms/select', 'result':'true'}
        return jsonify(result)

@app.route('/play/reset', methods=['POST'])   # 플레이 초기화
def playReset():
        jsonCnt = request.json['count']
        jsonPos = request.json['position']
        jsonHit = request.json['attack']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "update play set count = %s ,`position` = %s, attack = %s where play.login_idx = %s"
        curs.execute(sql, (jsonCnt, jsonPos, jsonHit, session['idx']))
        conn.commit()
        conn.close()
        result = {'url':'play/reset', 'result':'true'}
        return jsonify(result)

@app.route('/play/out', methods=['POST'])   # 방 나가기 요청
def playOut():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        
        sql = "select play.state from play where login_idx = %s"
        curs.execute(sql, (session['idx']))
        state = curs.fetchone()[0]
        
        sql = "select * from room where host = %s or guest = %s"
        curs.execute(sql, (session['idx'], session['idx']))
        data = curs.fetchall()
        
        for row in data:
            host = row[0]
            guest = row[1]
            
        if state == 1 or state == 2:   # 방 나갈시 패널티 ,상대방 보상 처리
            sql = "update ranking set lose = lose+1,rate = round(win/(win+lose)*100,2), score=1000+win*2-lose where ranking.user_idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
        
            if session['idx'] == host:
                sql = "update ranking set win = win+1,rate = round(win/(win+lose)*100,2), score=1000+win*2-lose where ranking.user_idx = %s"
                curs.execute(sql, (guest))
                conn.commit()
                
                sql = "update user set count = count+1 where user.idx = %s"
                curs.execute(sql, (guest))
                conn.commit()
                
            elif session['idx'] == guest:
                sql = "update ranking set win = win+1, rate = round(win/(win+lose)*100,2), score=1000+win*2-lose where ranking.user_idx = %s"
                curs.execute(sql, (host))
                conn.commit()
                
                sql = "update user set count = count+1 where user.idx = %s"
                curs.execute(sql, (host))
                conn.commit()
            
        if session['idx'] == host and guest == 0:   # 방 정보 갱신
            sql = "delete from room where host = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            result = {'url':'play/out', 'result':'true'}      
        elif session['idx'] == host:
            sql = "update room set host = %s , guest = 0 where host = %s"
            curs.execute(sql, (guest, host))
            conn.commit()
            result = {'url':'play/out', 'result':'true'}
        elif session['idx'] == guest:
            sql = "update room set guest = 0 where guest = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            result = {'url':'play/out', 'result':'true'}
            
        sql = "delete from play where login_idx = %s"
        curs.execute(sql, (session['idx']))
        conn.commit()
        
        conn.close()
        return jsonify(result)

@app.route('/play/ready', methods=['POST'])   # 게임 준비 요청
def playReady():
        jsonReady = request.json['ready']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        
        if jsonReady == "true":
            sql = "update play set ready = 1 where play.login_idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            result = {'url':'play/ready', 'result':'on'}
        else:
            sql = "update play set ready = 0, state = 0 where play.login_idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            result = {'url':'play/ready', 'result':'off'}
        
        conn.close()
        return jsonify(result)

@app.route('/play/change', methods=['POST'])   # 플레이 턴 전환
def playChange():
        jsonIdx = request.json['idx']
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "update play set state = case when login_idx = %s then 2 when login_idx = %s then 1 end where login_idx in (%s,%s)"
        curs.execute(sql, (session['idx'], jsonIdx, session['idx'], jsonIdx))
        conn.commit()

        sql = "update play set count = 3 where play.login_idx = %s"
        curs.execute(sql, (session['idx']))
        conn.commit()

        conn.close()
        result = {'url':'play/change', 'result':'true'}
        return jsonify(result)
      
@app.route('/play/result', methods=['POST'])   # 플레이 결과 처리
def playResult():
        jsonWin = request.json['win']

        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        
        sql = "update play set state = 0, ready = 0 where login_idx = %s"
        curs.execute(sql, (session['idx']))
        conn.commit()
        
        if jsonWin == "true":
            sql = "update ranking set win = win+1,rate = round(win/(win+lose)*100,2), score=1000+win*2-lose where ranking.user_idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            
            sql = "update user set coin = coin+100 where user.idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
        else:
            sql = "update ranking set lose = lose+1,rate = round(win/(win+lose)*100,2), score=1000+win*2-lose where ranking.user_idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            
            sql = "update user set coin = coin+50 where user.idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
        
        sql = "select `user`.count from `user` where user.idx = %s"
        curs.execute(sql, (session['idx']))
        count = curs.fetchone()[0]
        conn.close()
        
        result = {'url':'play/result', 'result':'true', 'count':count}
        return jsonify(result)

@app.route('/play/poll/match', methods=['POST'])   # 플레이 매칭 상태 확인
def playPollMatch():
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select * from room where host = %s or guest = %s"
        curs.execute(sql, (session['idx'], session['idx']))
        data = curs.fetchall()
        
        if data != ():
            for row in data:
                row1 = row[0]
                row2 = row[1]
        else:
            row2 = 0
       
        if row2 == 0:   # 매칭 실패
            result = {'url':'play/poll/match', 'result':'false'}      
        elif session['idx'] == row1:
            sql = "select user.nick from user where idx = %s"
            curs.execute(sql, (row2))
            nick = curs.fetchone()[0]
            result = {'url':'play/poll/match', 'result':'true', 'own':row1, 'enemy':row2 ,'nick':nick}
        elif session['idx'] == row2:
            sql = "select user.nick from user where idx = %s"
            curs.execute(sql, (row1))
            nick = curs.fetchone()[0]
            result = {'url':'play/poll/match', 'result':'true', 'own':row2, 'enemy':row1,'nick':nick}
              
        conn.close()      
        return jsonify(result)

@app.route('/play/poll/ready', methods=['POST'])   # 게임 준비 상태 확인
def playPollReady():
        jsonIdx = request.json['idx']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select play.ready,play.state from play where play.login_idx in(%s)"
        curs.execute(sql, (session['idx']))
        ownData = curs.fetchall()
        
        for row in ownData:
            ownReady = row[0]
            ownState = row[1]
        
        sql = "select play.ready,play.state from play where play.login_idx in(%s)"
        curs.execute(sql, (jsonIdx))
        enemData = curs.fetchall()
        
        if enemData != ():
            for row in enemData:
                enemReady = row[0]
                enemState = row[1]    

        if enemData == ():   # 게임 준비중 나감
            result = {'url':'play/poll/ready', 'result':'out'}
        elif ownReady == 1 and enemReady == 1:
            if ownState == 0 and enemState == 0:
                turn = random.choice([True, False])
                if turn == True:
                    sql = "update play set state = case when login_idx = %s then 1 when login_idx = %s then 2 end where login_idx in (%s,%s)"
                    curs.execute(sql, (session['idx'], jsonIdx, session['idx'], jsonIdx))
                    conn.commit()
                else:
                    sql = "update play set state = case when login_idx = %s then 2 when login_idx = %s then 1 end where login_idx in (%s,%s)"
                    curs.execute(sql, (session['idx'], jsonIdx, session['idx'], jsonIdx))
                    conn.commit()
                
                sql = "update user set count = case when idx = %s then count-1 when idx = %s then count-1 end where idx in (%s,%s)"
                curs.execute(sql, (session['idx'], jsonIdx, session['idx'], jsonIdx))
                conn.commit()     
                       
            result = {'url':'play/poll/ready', 'result':'true', 'ownReady':ownReady, 'ownState':ownState, 'enemReady':enemReady, 'enemState':enemState}  
        else:   # 게임 준비 대기상태 
            result = {'url':'play/poll/ready', 'result':'wait'}
 
        conn.close()
        return jsonify(result)   

@app.route('/play/poll/battle', methods=['POST'])   # 상대방 플레이 갱신
def playPollBattle():
        jsonIdx = request.json['idx']
        jsonCnt = request.json['count']
        
        conn = pymysql.connect(host=dbInfo['host'], port=dbInfo['port'], user=dbInfo['user'], password=dbInfo['password'], db=dbInfo['db'], charset=dbInfo['charset'])
        curs = conn.cursor()
        sql = "select play.state,play.count,play.position,play.attack from play where play.login_idx = %s"
        curs.execute(sql, (jsonIdx))
        data = curs.fetchall()
        
        if data != ():    
            for row in data:
                stat = row[0]
                cnt = row[1]
                pos = row[2]
                atk = row[3]
        
        if data == ():   # 상대방 나감
            sql = "update play set ready = 0,state = 0 where play.login_idx = %s"
            curs.execute(sql, (session['idx']))
            conn.commit()
            result = {'url':'play/poll/battle', 'result':'out'}
        elif jsonCnt == cnt:   # 갱신 미처리
            result = {'url':'play/poll/battle', 'result':'false'}   
        else:
            result = {'url':'play/poll/battle', 'result':'true', 'state':stat, 'count':cnt, 'position':pos, 'attack':atk}
            sql = "update play set attack='99,99,99,99,99,99,99,99,99,99,99' where play.login_idx = %s"
            curs.execute(sql, (jsonIdx))
            conn.commit() 
            
        conn.close()
        return jsonify(result)
    
if __name__ == "__main__":
    app.secret_key = os.urandom(12)
    app.run(host="0.0.0.0", port = 8910)