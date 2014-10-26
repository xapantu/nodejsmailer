net = require 'net'
imap = require './imap.js'
starttls = require './starttls.js'


server = net.createServer (c) -> #'connection' listener
	console.log 'server connected'
	
	wait_for_more = null
	must_quit = false
	ssl = false

	handle_command = (cmd) ->
		if cmd.length < 2
			"* You have to handle uids of the queries…"
		else
			switch cmd[1].toLowerCase()
				when 'capability'
					if c.ssl
						'* CAPABILITY IMAP4rev1 AUTH=EXTERNAL\r\n' +
						cmd[0] + " OK CAPABILITY completed\r\n"
					else
						'* CAPABILITY IMAP4rev1 STARTTLS LOGINDISABLED\r\n' +
						cmd[0] + " OK CAPABILITY completed\r\n"
				when 'authenticate', 'AUTHENTICATE'
					wait_for_auth = (e) ->
						[cmd[0] + " OK AUTHENTICATE completed\r\n", '']
					wait_for_more = wait_for_auth
					'+\n\r'
				when 'logout'
					must_quit = true
					null
				when 'starttls'
					starttls.starttls c, cmd[0]
				when 'login'
					if cmd.length != 4
						"BAD login command"
					else
						cmd[0] + " OK LOGIN completed\r\n"
				when 'list'
					cmd[0] + " OK list completed\r\n"
				when 'lsub'
					cmd[0] + " OK lsub completed\r\n"
				when 'create'
					cmd[0] + " OK create completed\r\n"
				when 'select'
					cmd[0] + " OK select completed\r\n"
				when 'noop'
					cmd[0] + " OK noop completed\r\n"
				else '* OK'

	
	c.on 'end', () ->
		console.log 'server disconnected'
	
	#c.on 'error', (e) ->
	#	console.log e

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


	c.write '* OK\n\r'
	#c.pipe c

server.listen 1430, () -> #'listening' listener
	console.log 'server bound'

