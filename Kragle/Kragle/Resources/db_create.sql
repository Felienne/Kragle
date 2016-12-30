CREATE TABLE IF NOT EXISTS proto_user
(
	id INTEGER NOT NULL,
	username TEXT NOT NULL,

	PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS user_project
(
	user_id INTEGER NOT NULL,
	project_id INTEGER NOT NULL,

	PRIMARY KEY (user_id, project_id)
);

CREATE TABLE IF NOT EXISTS project
(
	project_id INTEGER NOT NULL,

	PRIMARY KEY (project_id)
);

CREATE TABLE IF NOT EXISTS proto_code
(
	project_id INTEGER NOT NULL,
	fetch_date REAL NOT NULL,
	raw_contents TEXT,

	PRIMARY KEY (project_id, fetch_date)
);
