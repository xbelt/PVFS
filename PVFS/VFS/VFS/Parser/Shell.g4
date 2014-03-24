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
 //TODO: we do not yet support paths with whitespaces
compileUnit
	:	EOF																									#Eof
	| 'ls' files=F? dirs=D? compileUnit																		#Ls
	| 'cd' path=Path? ident=Identifier? dots='..'? compileUnit												#Cd
	| ('copy' | 'cp') opt=R? src=Path dst=Path compileUnit													#Cp
	| ('createdisk' | 'cdisk') par1=optParams* '-s' (Integer SizeUnit | Size)  par2=optParams* compileUnit	#Cdisk
	| ('removedisk' | 'rmdisk') ('-p' sys=SysPath | '-n' name=Identifier)+ compileUnit						#Rmdisk
	| ('loaddisk' | 'ldisk') ('-p' sys=SysPath | '-n' name=Identifier)+ compileUnit							#Ldisk
	| ('listdisks' | 'ldisks') ('-p' sys=SysPath)? compileUnit												#Ldisks
	| 'mkdir' (id=Identifier | path=Path) compileUnit														#Mkdir
	| ('mk' | 'touch') (id=Identifier | path=Path) compileUnit												#MkFile
	| ('remove' | 'rm') (trgt=Path | id=Identifier) compileUnit												#Rm
	| ('move' | 'mv') src=Identifier dst=(Path | Identifier) compileUnit															#Mv
	| ('import' | 'im') ext=SysPath inte=Path compileUnit													#Im
	| ('export' | 'ex') inte=Path ext=SysPath compileUnit													#Ex
	| ('rename' | 'rn') src=Identifier dst=Identifier compileUnit								#Rn
	| 'free'																								#Free
	| 'occ'																									#Occ
	| ('exit' | 'quit')																						#Exit
	;

optParams
	: ('-p' path=SysPath																								
	| '-n' name=Identifier
	| '-b' block=Integer)
	;
/*
 * Lexer Rules
 */

 R : '-R';
 F : '-f';
 D : '-d';

 SizeUnit
	:'kb' | 'mb' | 'gb' | 'tb' | 'KB' | 'MB' | 'GB' | 'TB';

Size
	: Integer SizeUnit
	;
 Integer
	: [0-9]+
	;

fragment
String
	: [a-zA-Z0-9_]+
	;

 Identifier
	: String ('.' String)* ('/' Identifier)*
	;

Path
	: ('/' String)+ ('/')? ('/' Identifier)?
	| '/'
	;

SysPath
	: [a-zA-Z]+ ':' ('\\' String)* ('\\')? ('\\' Identifier)?
	;

WS
	:	' ' -> channel(HIDDEN)
	;
