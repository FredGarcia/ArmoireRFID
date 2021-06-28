-- phpMyAdmin SQL Dump
-- version 3.5.1
-- http://www.phpmyadmin.net
--
-- Client: localhost
-- Généré le: Jeu 26 Septembre 2013 à 16:00
-- Version du serveur: 5.5.24-log
-- Version de PHP: 5.4.3

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Base de données: `coding01v3`
--

-- --------------------------------------------------------

--
-- Structure de la table `alert`
--

CREATE TABLE IF NOT EXISTS `alert` (
  `Id` int(8) NOT NULL AUTO_INCREMENT,
  `Date_Creation` datetime NOT NULL,
  `Alert_Type_Id` int(8) NOT NULL,
  `Message` varchar(2500) NOT NULL,
  `User_ID` int(32) NOT NULL,
  `Armoire_ID` int(32) NOT NULL,
  `Traiter` smallint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=292 ;

-- --------------------------------------------------------

--
-- Structure de la table `alert_type`
--

CREATE TABLE IF NOT EXISTS `alert_type` (
  `Id` int(11) NOT NULL,
  `Type` varchar(255) NOT NULL,
  `Code` varchar(255) NOT NULL,
  `Description` varchar(255) NOT NULL,
  `Niveau` varchar(255) NOT NULL,
  `Contact` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `armoire`
--

CREATE TABLE IF NOT EXISTS `armoire` (
  `Id` int(8) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Date_Modification` datetime NOT NULL,
  `Nb_Case` int(8) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `IP` varchar(255) NOT NULL,
  `Id_Client` int(32) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `article_taille`
--

CREATE TABLE IF NOT EXISTS `article_taille` (
  `Article_Type_ID` int(8) NOT NULL COMMENT 'Id du type article correspondant au vêtement',
  `Taille` varchar(255) NOT NULL COMMENT 'Taille pour l''article donné',
  `Armoire` int(8) NOT NULL COMMENT 'Numérotation de l''armoire',
  `Vide` tinyint(1) NOT NULL COMMENT 'indique si le type d''article/taille est épuisé',
  PRIMARY KEY (`Article_Type_ID`,`Taille`,`Armoire`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `article_type`
--

CREATE TABLE IF NOT EXISTS `article_type` (
  `Id` int(8) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Date_Modification` datetime NOT NULL,
  `Code` varchar(255) NOT NULL,
  `Type_Taille` char(1) NOT NULL DEFAULT '1',
  `Description` varchar(255) NOT NULL,
  `Couleur` varchar(255) NOT NULL,
  `Sexe` varchar(255) NOT NULL,
  `Photo` varchar(255) NOT NULL,
  `Active` int(8) NOT NULL,
  `Description_longue` varchar(255) NOT NULL COMMENT 'Intitulé qui est affiché dans l''application Web',
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `case`
--

CREATE TABLE IF NOT EXISTS `case` (
  `Id` int(8) NOT NULL,
  `Bind_ID` int(32) NOT NULL,
  `Taille` varchar(255) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Max_Item` int(8) NOT NULL,
  `Article_Type_Id` int(8) NOT NULL,
  `Armoire_ID` int(8) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `content_arm`
--

CREATE TABLE IF NOT EXISTS `content_arm` (
  `Id` int(8) NOT NULL AUTO_INCREMENT,
  `Creation_Date` datetime NOT NULL,
  `Epc` varchar(255) NOT NULL,
  `State` int(3) NOT NULL,
  `RFID_Reader` int(60) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=8753 ;

-- --------------------------------------------------------

--
-- Structure de la table `corresp_lang`
--

CREATE TABLE IF NOT EXISTS `corresp_lang` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `cultureName` varchar(255) NOT NULL,
  `Language` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

-- --------------------------------------------------------

--
-- Structure de la table `corresp_taille`
--

CREATE TABLE IF NOT EXISTS `corresp_taille` (
  `Type-Taille` char(1) NOT NULL,
  `Taille` varchar(255) NOT NULL,
  `Classement_tailles` int(11) NOT NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `epc`
--

CREATE TABLE IF NOT EXISTS `epc` (
  `Id` int(8) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Date_Modification` datetime NOT NULL,
  `Tag` varchar(255) NOT NULL,
  `Code_Barre` varchar(255) NOT NULL,
  `Taille` varchar(255) NOT NULL,
  `Type_Taille` char(1) NOT NULL,
  `Cycle_Lavage_Count` int(8) NOT NULL,
  `State` int(8) NOT NULL,
  `Last_User` varchar(255) NOT NULL,
  `Last_Reader` varchar(255) NOT NULL,
  `Last_Action` varchar(255) NOT NULL,
  `Last_Action_Date` datetime NOT NULL,
  `Movement` int(8) NOT NULL,
  `Article_Type_ID` int(8) NOT NULL,
  `Case_ID` int(8) NOT NULL,
  `Armoire_ID` int(8) NOT NULL,
  `Actif` int(8) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

--
-- Déclencheurs `epc`
--
DROP TRIGGER IF EXISTS `Insert_Log_Epc`;
DELIMITER //
CREATE TRIGGER `Insert_Log_Epc` BEFORE UPDATE ON `epc`
 FOR EACH ROW BEGIN
IF (OLD.State != NEW.State)
THEN INSERT INTO log_epc (Date_creation,Epc_Id,Tag,Code_Barre,Taille,Cycle_Lavage_Count,State,Last_User,Last_Reader,Last_Action,Last_Action_Date,Movement,Article_Type_ID,Case_ID,Armoire_ID) VALUES (OLD.Date_Modification,OLD.Id,OLD.Tag,OLD.Code_Barre, OLD.Taille,OLD.Cycle_Lavage_Count,OLD.State, OLD.Last_User,OLD.Last_Reader,NEW.Last_Action,OLD.Last_Action_Date,OLD.Movement,OLD.Article_Type_ID,OLD.Case_ID,OLD.Armoire_ID);
        END IF;
   END
//
DELIMITER ;

-- --------------------------------------------------------

--
-- Structure de la table `log_epc`
--

CREATE TABLE IF NOT EXISTS `log_epc` (
  `Id` int(8) NOT NULL AUTO_INCREMENT,
  `Date_Creation` datetime NOT NULL,
  `Epc_Id` int(11) NOT NULL,
  `Tag` varchar(255) NOT NULL,
  `Code_Barre` varchar(255) NOT NULL,
  `Taille` varchar(255) NOT NULL,
  `Cycle_Lavage_Count` int(8) NOT NULL,
  `State` int(8) NOT NULL,
  `Last_User` varchar(255) NOT NULL,
  `Last_Reader` varchar(255) NOT NULL,
  `Last_Action` varchar(255) NOT NULL,
  `Last_Action_Date` datetime NOT NULL,
  `Movement` int(8) NOT NULL,
  `Article_Type_ID` int(8) NOT NULL,
  `Case_ID` int(8) NOT NULL,
  `Armoire_ID` int(8) NOT NULL,
  `Synchronised` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=679 ;

-- --------------------------------------------------------

--
-- Structure de la table `state`
--

CREATE TABLE IF NOT EXISTS `state` (
  `STATE_ID` int(11) NOT NULL,
  `DESCRIPTION` varchar(50) NOT NULL,
  `NEXT_STATE` int(11) NOT NULL,
  PRIMARY KEY (`STATE_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `tag_alert`
--

CREATE TABLE IF NOT EXISTS `tag_alert` (
  `Id` int(32) NOT NULL AUTO_INCREMENT,
  `Date_Creation` datetime NOT NULL,
  `Alert_ID` int(32) NOT NULL,
  `Tag_Command` varchar(255) NOT NULL,
  `Tag_Retir` varchar(255) NOT NULL,
  `Tag_Intrus` varchar(255) NOT NULL,
  `Article_Type_Code` varchar(255) NOT NULL,
  `Taille` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin1 AUTO_INCREMENT=35 ;

-- --------------------------------------------------------

--
-- Structure de la table `user`
--

CREATE TABLE IF NOT EXISTS `user` (
  `Id` int(8) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Date_Modification` datetime NOT NULL,
  `Login` varchar(255) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Type` varchar(255) NOT NULL,
  `Nom` varchar(255) NOT NULL,
  `Prenom` varchar(255) NOT NULL,
  `Id_Lang` int(11) NOT NULL DEFAULT '89',
  `Sexe` varchar(255) NOT NULL,
  `Taille` varchar(255) NOT NULL,
  `Groupe` int(8) NOT NULL,
  `Department` int(8) NOT NULL,
  `Photo` varchar(255) NOT NULL,
  `Last_Connection` datetime NOT NULL,
  `Active` int(2) NOT NULL,
  `End_of_Validity` datetime NOT NULL COMMENT 'Date de fin d''activité',
  `Wearer_Code` varchar(255) NOT NULL COMMENT 'Code d''identification sur le serveur ATLAS',
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `user_article`
--

CREATE TABLE IF NOT EXISTS `user_article` (
  `Id` int(8) NOT NULL,
  `Date_Creation` datetime NOT NULL,
  `Date_Modification` datetime NOT NULL,
  `Taille` varchar(255) NOT NULL,
  `Credit` int(8) NOT NULL,
  `Credit_Restant` int(8) NOT NULL,
  `User_Id` int(8) NOT NULL,
  `Article_Type_Id` int(8) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `version`
--

CREATE TABLE IF NOT EXISTS `version` (
  `Id` int(8) NOT NULL COMMENT 'Id de l''armoire',
  `NomPlace` varchar(255) NOT NULL COMMENT 'Défini le nom complet et l''emplacement de l''armoire',
  `VersLog` varchar(255) NOT NULL COMMENT 'Version logiciel',
  `VersMat` varchar(255) NOT NULL COMMENT 'Version matériel',
  `DateSynchro` datetime NOT NULL COMMENT 'Date de la dernière synchro',
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Structure de la table `_heartbeat`
--

CREATE TABLE IF NOT EXISTS `_heartbeat` (
  `SYSID` int(11) NOT NULL,
  `HBDATE` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `SYNCHRODATE` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `STATE` text NOT NULL,
  `DETAILS` varchar(255) NOT NULL,
  PRIMARY KEY (`SYSID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
