tls = require "tls"
crypto = require "crypto"
fs = require "fs"
emit = require('events').EventEmitter.prototype.emit

events = ['error','close','data']

module.exports.starttls = (connection, tag) ->
	cred = crypto.createCredentials {key:  (fs.readFileSync "server-key.pem")
									,cert: (fs.readFileSync "server-cert.pem")}

	# TODO: investigate wether this is robust enough
	# -- xapantu dimanche 26 octobre 2014, 23:00:00 (UTC+0100)
	securePair = tls.createSecurePair cred, true, false, false

	clearText = securePair.cleartext

	old_events = {}
	
	# Disconnect everything so as it does not interfere with the TLS handshake
	# We'll reconnect later with the encryption enabled
	for event in events
		old_events[event] = (connection.listeners(event) || [])
		connection.removeAllListeners(event)

	# Some magic
	clearText.socket = connection
	clearText.encrypted = securePair.encrypted
	clearText.authorized = false
	securePair.on 'error', emit.bind clearText, 'error'
	securePair.on 'data', (err, data) ->
		console.log err, data

	securePair.on 'secure', () ->
		verifyError = (securePair.ssl || securePair._ssl).verifyError()
		
		if verifyError
			clearText.authorized = false
			clearText.authorizationError = verifyError
		else
			clearText.authorized = true

		# Reconnect the other, unencrypted listener (that are now crypted)
		for event in events
			listeners = old_events[event]
			for listener in listeners
				clearText.on event, listener
		
		connection.stream = clearText
		connection.ssl = true

	# Some magic
	clearText._controlReleased = true
	connection.pipe securePair.encrypted
	securePair.encrypted.pipe connection

	tag + ' OK Begin TLS negotiation now\r\n'
