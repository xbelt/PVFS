grammar Shell;

@parser::members
{
	protected const int EOF = Eof;
}

@lexer::members
{
	protected const int EOF = Eof;
	protected const int HIDDEN = Hidden;
}

/*
 * Parser Rules
 */

compileUnit
	:	EOF																							#Eof
	| 'ls' compileUnit																				#Ls
	| 'cd' Path compileUnit																			#Cd
	| ('copy' | 'cp') ('-R')? Path Path compileUnit													#Cp
	| ('createdisk' | 'cdisk') optParams* '-s' (Integer SizeUnit | Size)  optParams* compileUnit	#Cdisk
	| ('removedisk' | 'rmdisk') ('-p' SysPath | '-n' String)+										#Rmdisk
	| 'mkdir' (Path | Identifier)																	#Mkdir
	| ('remove' | 'rm') ('-R')? (Path | Identifier)+												#Rm
	| ('move' | 'mv') ('-R')? (Path | Identifier) (Path | Identifier)								#Mv
	| ('import' | 'im') SysPath (Path | Identifier)													#Im
	| ('export' | 'ex') (Path | Identifier) SysPath													#Ex
	| 'free'																						#Free
	| 'occ'																							#Occ
	;

optParams
	: '-p' Path																						#PathParam
	| '-n' String																					#StringParam
	;
/*
 * Lexer Rules
 */
 SizeUnit
	:'kb' | 'mb' | 'gb' | 'tb' | 'KB' | 'MB' | 'GB' | 'TB';

Size
	: Integer SizeUnit
	;
 Integer
	: [0-9]+
	;

 Identifier
	: String '.' String
	;

String
	: [a-zA-Z0-9]+
	;

Path
	: ('/' String)+ ('/')?
	| '/'
	;

SysPath
	: [a-zA-Z]+ ':' ('\\' String)* ('\\')?
	;

WS
	:	' ' -> channel(HIDDEN)
	;
