parser = require './imap.js'

exports.test_parser = (t) ->

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
