Sequelize = require 'sequelize'
async = require 'async'

class DataBase
	constructor: (@path) ->
		@sequelize = new Sequelize null, null, null, { dialect: 'sqlite', storage:@path }
	

		@user = @sequelize.define 'User',
			{ username: Sequelize.STRING
			, password: Sequelize.STRING
			, group: Sequelize.STRING
			}

		@mailbox = @sequelize.define 'Mailbox',
			{ name: Sequelize.STRING
			}

		@user.hasMany @mailbox

		@mailbox.belongsTo @user



	sync: (done) ->
		@sequelize.sync()
			.complete (err) ->
				if err
					console.log "Error while syncing database: " + err
				else
					done()
	connect: (done) ->
		t = @
		@sequelize.authenticate()
			.complete (err) ->
				if err
					console.log "Error, couldn't authenticate to the database: " + err
				else
					t.sync(done)
	
	add_user : (username, password, done) ->

		# create user
		me = @user.build { username:username
				   , password:password
				   }
		t = @

		me.save().complete () ->
			# create default inbox
			inbox = t.mailbox.build { name: 'inbox' }
			me.addMailbox inbox

			inbox.save().complete (err) ->
				if err
					console.log err
				# callback
				else if done
					done(me)
	
	select_mailbox : (user, mailbox_name, done) ->
		user.getMailboxes(where: name:mailbox_name).complete (err, mailboxes) ->
			if err
				console.log err
			else
				done (mailboxes[0] if mailboxes.length > 0)
		
	

	
module.exports =
	DataBase: DataBase



