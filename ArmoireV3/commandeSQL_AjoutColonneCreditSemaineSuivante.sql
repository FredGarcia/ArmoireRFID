delimiter |
DROP PROCEDURE IF EXISTS ajout_colonne |
CREATE PROCEDURE ajout_colonne ()
	BEGIN
		IF NOT EXISTS( (SELECT * FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE()
				AND COLUMN_NAME='Credit_Semaine_Suivante' AND TABLE_NAME='user_article') ) THEN
			ALTER TABLE `user_article` ADD `Credit_Semaine_Suivante` INT( 8 ) NOT NULL COMMENT 'Nombre de crédits à ajouter aux crédits restants la semaine suivante' AFTER `Credit_Restant` ;
		END IF;
	end|
CALL ajout_colonne() |
delimiter ;
