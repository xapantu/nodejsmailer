all: gr main.js db.js starttls.js tests.js tests

serve: all server-key.pem server-cert.pem
	coffee imap-server.cs

server-key.pem:
	openssl genrsa -out server-key.pem 1024

server-cert.pem: server-key.pem server-csr.pem
	openssl x509 -req -in server-csr.pem -signkey server-key.pem -out server-cert.pem

server-csr.pem: server-key.pem
	openssl req -new -key server-key.pem -out server-csr.pem

main.js: main.cs
	coffee -c main.cs

starttls.js: starttls.cs
	coffee -c starttls.cs


tests.js: tests.cs
	coffee -c tests.cs

db.js: db.cs
	coffee -c db.cs


tests:
	nodeunit tests.js

gr:
	jison imap.gr
