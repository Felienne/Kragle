CREATE TABLE IF NOT EXISTS scratch_user
(
	-- The unique id of the user
	user_id INTEGER NOT NULL,

	-- The username of the user
	username TEXT NOT NULL,
	-- The date the user joined
	join_date BIGINT,
	-- The (supposed) country of residence
	country VARCHAR(255),

	PRIMARY KEY (user_id)
);

CREATE TABLE IF NOT EXISTS user_project
(
	user_id INTEGER NOT NULL,
	project_id INTEGER NOT NULL,

	PRIMARY KEY (user_id, project_id)
);

CREATE TABLE IF NOT EXISTS project
(
	-- The unique id of the project
	project_id INTEGER NOT NULL,

	-- The title of the project
	title VARCHAR(255),
	-- The date the project was created
	create_date BIGINT,
	-- The date the project was last modified
	modify_date BIGINT,
	-- The date the project was publicly shared
	share_date BIGINT,
	-- The number of views
	view_count INTEGER,
	-- The number of loves given
	love_count INTEGER,
	-- The number of people that have favorited this project
	favorite_count INTEGER,
	-- The number of comments on the project
	comment_count INTEGER,
	-- If this project is a remix, this is the id of the "parent" project that was remixed
	remix_parent_id INTEGER,
	-- If this project belongs to a chain of remixes, this is the id of the original project
	remix_root_id INTEGER,

	PRIMARY KEY (project_id)
);

CREATE TABLE IF NOT EXISTS project_code
(
	-- The id of the project
	project_id INTEGER NOT NULL,
	-- The date the code was read from the website
	fetch_date BIGINT NOT NULL,

	-- The code of the project in JSON format
	code TEXT,

	PRIMARY KEY (project_id, fetch_date)
);
