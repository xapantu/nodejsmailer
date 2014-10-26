Sequelize = require 'sequelize'

sequelize = new Sequelize null, null, null, { dialect: 'sqlite', storage:'index.sqlite3' }
user = sequelize.define 'User',
	{ username: Sequelize.STRING
	, password: Sequelize.STRING
	, group: Sequelize.STRING
	}


sync = () ->
	sequelize.sync()
		.complete (err) ->
			console.log err
			console.log 'synced'
	
module.exports =
	load_db : () ->
		sequelize.authenticate()
			.complete (err) ->
				console.log err
				console.log 'connected'
				sync()
	sample_user : () ->
		me = user.build { username:'xapantu'
				   , password:'xapantu'
				   }
		me.save()
			.complete (err) ->
				sequelize.close()
	user: user



