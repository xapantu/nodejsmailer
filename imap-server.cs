net = require 'net'
imap = require './imap.js'
starttls = require './starttls.js'
db = require './db.js'

db.load_db()

# cmd: an array of an imap command, e.g. ['aa0', 'login', 'username', 'password']
# where aa0 is the id of the request, and must be sent when the response is complete
# assume cmd is correct
imap_reply =
	end : (cmd) ->
		cmd[0] + " OK " + cmd[1].toUpperCase() + " completed\r\n"
	more : (cmd) ->
		"+\r\n"
	capability: (cmd, ssl) ->
		if ssl
			'* CAPABILITY IMAP4rev1 AUTH=EXTERNAL\r\n'
		else
			'* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n'
	ok: () ->
		'* OK\r\n'
	
	bad: (cmd) ->
		cmd[0] + " BAD " + cmd[1].toUpperCase() + "\r\n"
	
	no_query_id: () ->
		 "* BAD You have to handle uids of the queries…\r\n"
		


server = net.createServer (c) -> #'connection' listener
	console.log 'server connected'
	
	wait_for_more = null
	must_quit = false
	ssl = false
	logged = false
	username = false

	# cmd: an array of an imap command, e.g. ['aa0', 'login', 'username', 'password']
	# where aa0 is the id of the request, and must be sent when the response is complete
	# send: a function that takes str and sends the result to the client
	handle_command = (cmd, send) ->
		if cmd.length < 2
			send imap_reply.no_query_id
		else
			switch cmd[1].toLowerCase()
				when 'capability'
					send (imap_reply.capability cmd, c.ssl) + (imap_reply.end cmd)
				when 'logout'
					must_quit = true
					send imap_reply.ok()
					null
				when 'starttls'
					send starttls.starttls c, cmd[0]
				when 'login'
					if cmd.length != 4
						send imap_reply.bad cmd
					else
						db.user.find where: username: cmd[2]
							.complete (err, user) ->
								if user and user.password == cmd[3]
									send imap_reply.end cmd
								else
									send imap_reply.bad cmd
				when 'list'
					send imap_reply.end cmd
				when 'lsub'
					send imap_reply.end cmd
				when 'create'
					send imap_reply.end cmd
				when 'select'
					send imap_reply.end cmd
				when 'noop'
					send imap_reply.end cmd
				when 'subscribe'
					send imap_reply.end cmd
				else
					send imap_reply.ok()

	
	c.on 'end', () ->
		console.log 'server disconnected'
	
	c.on 'error', (e) ->
		console.log e

	c.on 'data', (e) ->
		console.log ">>>> Reçu : " + e.toString()

		reply = null
		
		send = (reply) ->
			if reply
				console.log "<<<< Envoi : " + reply
				if c.ssl
					c.stream.write reply
				else
					c.write reply

			if must_quit
				c.end()

		if wait_for_more
			tmp = wait_for_more
			wait_for_more = null
			tmp_reject = reject
			reject = null
			tmp (e.toString() + tmp_reject)

		else
			# Extract the identifier of the command
			[cmds, reject] = imap.parse (e.toString() + reject)
			handle_command cmd, send for cmd in cmds



	# Show we're alive, some clients don't start, event if it is not in the spec (?)
	c.write imap_reply.ok()

	# What's the use of this thing?
	# c.pipe c

server.listen 1430, () -> #'listening' listener
	console.log 'server bound'

