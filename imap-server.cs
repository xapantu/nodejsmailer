net = require 'net'
imap = require './imap.js'
starttls = require './starttls.js'

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


server = net.createServer (c) -> #'connection' listener
	console.log 'server connected'
	
	wait_for_more = null
	must_quit = false
	ssl = false
	logged = false
	username = false

	# cmd: an array of an imap command, e.g. ['aa0', 'login', 'username', 'password']
	# where aa0 is the id of the request, and must be sent when the response is complete
	handle_command = (cmd) ->
		if cmd.length < 2
			"* You have to handle uids of the queries…"
		else
			switch cmd[1].toLowerCase()
				when 'capability'
					(imap_reply.capability cmd, c.ssl) + (imap_reply.end cmd)
				when 'logout'
					must_quit = true
					null
				when 'starttls'
					starttls.starttls c, cmd[0]
				when 'login'
					if cmd.length != 4
						"BAD login command"
					else
						imap_reply.end cmd
				when 'list'
					imap_reply.end cmd
				when 'lsub'
					imap_reply.end cmd
				when 'create'
					imap_reply.end cmd
				when 'select'
					imap_reply.end cmd
				when 'noop'
					imap_reply.end cmd
				else
					imap_reply.ok()

	
	c.on 'end', () ->
		console.log 'server disconnected'
	
	c.on 'error', (e) ->
		console.log e

	c.on 'data', (e) ->
		console.log ">>>> Reçu : " + e.toString()

		reply = null

		if wait_for_more
			tmp = wait_for_more
			wait_for_more = null
			[reply, reject] = tmp (e.toString() + reject)

		else
			# Extract the identifier of the command
			[cmds, reject] = imap.parse (e.toString() + reject)
			reply = [handle_command cmd for cmd in cmds].join('')

		if reply
			console.log "<<<< Envoi : " + reply
			if c.ssl
				c.stream.write reply
			else
				c.write reply

		if must_quit
			c.end()


	# Show we're alive, some clients don't start, event if it is not in the spec (?)
	c.write imap_reply.ok()

	# What's the use of this thing?
	# c.pipe c

server.listen 1430, () -> #'listening' listener
	console.log 'server bound'

