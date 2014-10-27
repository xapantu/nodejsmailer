module.exports =

	test_parser : (t) ->
		parser = require './imap.js'

		t.deepEqual (parser.parse '1 CAPABILITY\r\n2 LOGOUT\r\n')
				, [[['1', 'CAPABILITY'], ['2', 'LOGOUT']], ""]

		t.deepEqual (parser.parse '1 CAPABILITY\r\n2 LOGOUT')
				, [[['1', 'CAPABILITY']], "2 LOGOUT"]
		
		t.deepEqual (parser.parse '1 login "me" "pass"\r\n')
				, [[['1', 'login', 'me', 'pass']], ""]
		
		t.deepEqual (parser.parse '1 login "me" "fancy pass"\r\n')
				, [[['1', 'login', 'me', 'fancy pass']], ""]
		
		t.deepEqual (parser.parse '1 list "*"\r\n')
				, [[['1', 'list', '*']], ""]
		
		
		t.done()


	test_db : (t) ->
		db = require './db.js'

		t.expect 3

		index1 = new db.DataBase ':memory:'

		index1.connect () ->
			index1.add_user 'username', 'password', (uid) ->
				t.ok uid
				index1.select_mailbox uid, 'inbox', (m) ->
					t.ok m
					index1.select_mailbox uid, 'inbox2', (m) ->
						t.ok not m
						t.done()
