-- migrate:up

CREATE TABLE `seed` (
  `idx` varchar(255) NOT NULL COMMENT 'Index identifier',
  `val` bigint(20) NOT NULL DEFAULT 0 COMMENT 'Seed value',
  PRIMARY KEY (`idx`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Index seeds';

CREATE TABLE `version` (
  `val` char(32) NOT NULL COMMENT 'Version value',
  `reg_dt` datetime NOT NULL COMMENT 'Registration date-time',
  PRIMARY KEY (`val`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Index versions';

-- migrate:down

DROP TABLE `seed`, `version`;