-- phpMyAdmin SQL Dump
-- version 4.0.5
-- http://www.phpmyadmin.net
--
-- 호스트: localhost
-- 처리한 시간: 19-05-25 10:17
-- 서버 버전: 5.6.13
-- PHP 버전: 5.5.3

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- 데이터베이스: `animalchess`
--

-- --------------------------------------------------------

--
-- 테이블 구조 `chat`
--

CREATE TABLE IF NOT EXISTS `chat` (
  `login_idx` int(11) NOT NULL,
  `message` varchar(80) NOT NULL,
  `time` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- 테이블의 덤프 데이터 `chat`
--

INSERT INTO `chat` (`login_idx`, `message`, `time`) VALUES
(2, '안녕하세요', '2019-05-22 20:38:17'),
(1, '안녕하세요', '2019-05-22 20:38:26');

-- --------------------------------------------------------

--
-- 테이블 구조 `login`
--

CREATE TABLE IF NOT EXISTS `login` (
  `user_idx` int(11) NOT NULL,
  `session` varchar(100) NOT NULL,
  PRIMARY KEY (`user_idx`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- 테이블 구조 `play`
--

CREATE TABLE IF NOT EXISTS `play` (
  `login_idx` int(11) NOT NULL,
  `ready` tinyint(1) NOT NULL,
  `state` int(11) NOT NULL,
  `count` int(11) NOT NULL,
  `position` varchar(50) NOT NULL,
  `attack` varchar(50) NOT NULL,
  PRIMARY KEY (`login_idx`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- 테이블 구조 `ranking`
--

CREATE TABLE IF NOT EXISTS `ranking` (
  `user_idx` int(11) NOT NULL AUTO_INCREMENT,
  `win` int(11) NOT NULL,
  `lose` int(11) NOT NULL,
  `rate` float NOT NULL,
  `score` int(11) NOT NULL DEFAULT '1000',
  PRIMARY KEY (`user_idx`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4 AUTO_INCREMENT=15 ;

--
-- 테이블의 덤프 데이터 `ranking`
--

INSERT INTO `ranking` (`user_idx`, `win`, `lose`, `rate`, `score`) VALUES
(1, 60, 100, 37.5, 1020),
(2, 112, 85, 56.85, 1139),
(3, 18, 12, 60, 1024),
(4, 2, 0, 100, 1004),
(7, 2, 0, 100, 1004),
(8, 0, 1, 0, 999),
(9, 2, 0, 100, 1004),
(10, 2, 0, 100, 1004),
(11, 2, 0, 100, 1004),
(12, 2, 0, 100, 1004),
(13, 2, 0, 100, 1004),
(14, 0, 0, 0, 1000);

-- --------------------------------------------------------

--
-- 테이블 구조 `room`
--

CREATE TABLE IF NOT EXISTS `room` (
  `host` int(11) NOT NULL,
  `guest` int(11) NOT NULL,
  PRIMARY KEY (`host`,`guest`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------

--
-- 테이블 구조 `user`
--

CREATE TABLE IF NOT EXISTS `user` (
  `idx` int(11) NOT NULL AUTO_INCREMENT,
  `id` varchar(20) NOT NULL,
  `passwd` varchar(100) NOT NULL,
  `nick` varchar(20) NOT NULL,
  `coin` int(11) NOT NULL DEFAULT '1000',
  `count` int(11) NOT NULL DEFAULT '80',
  `log` date NOT NULL,
  PRIMARY KEY (`idx`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4 ROW_FORMAT=COMPACT AUTO_INCREMENT=15 ;

--
-- 테이블의 덤프 데이터 `user`
--

INSERT INTO `user` (`idx`, `id`, `passwd`, `nick`, `coin`, `count`, `log`) VALUES
(1, 'test2', '*7CEB3FDE5F7A9C4CE5FBE610D7D8EDA62EBE5F4E', '테스트2', 450, 78, '2019-05-22'),
(2, 'test1', '*06C0BF5B64ECE2F648B5F048A71903906BA08E5C', '테스트1', 1950, 77, '2019-05-22'),
(3, 'test3', '*F357E78CABAD76FD3F1018EF85D78499B6ACC431', '테스트3', 1050, 80, '2019-05-21'),
(4, 'test4', '*D159BBDA31273BE3F4F00715B4A439925C6A0F2D', '테스트4', 1100, 79, '0000-00-00'),
(7, 'test5', '*30B3620A8C3D75549E8B7F077424EF88B6C798E6', '테스트5', 900, 80, '0000-00-00'),
(8, 'test6', '*CFB0272DC9E549723E685BB74CBC3D05E4C2AF54', '테스트6', 800, 80, '0000-00-00'),
(9, 'test7', '*DBB670647A544F1A6C7715B6CEB0B386518E30B8', 'test7', 900, 80, '0000-00-00'),
(10, 'test8', '*F88810FD53132CA89291BA2AE8FD63D5A9F031FA', 'test8', 900, 80, '0000-00-00'),
(11, 'test9', '*34521800E7C207ED39F616B4132496F98D6C2A9F', 'test9', 900, 80, '0000-00-00'),
(12, 'test10', '*A112739E5B6A174DC9A8C9D20657467B3A64A5A7', 'test10', 900, 80, '0000-00-00'),
(13, 'test', '*94BDCEBE19083CE2A1F959FD02F964C7AF4CFC29', 'test', 700, 81, '0000-00-00'),
(14, 'ttt', '*A0C1B1AEC5E4FC2670F87F7F6A46ACF06DC15605', 'ttt', 1000, 80, '2019-05-21');

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
