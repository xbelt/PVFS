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
	:	EOF																									#Eof
	| ('createdisk' | 'cdisk') (par1=optParams)* '-s' (Integer SizeUnit | Size)  par2=optParams* compileUnit	#Cdisk
	| ('loaddisk' | 'ldisk') ( sys=SysPath | name=Identifier)+ ('-pw' pw=Identifier)? compileUnit			#Ldisk
	| ('unloaddisk' | 'udisk') name=Identifier compileUnit													#Udisk
	| ('removedisk' | 'rmdisk') (sys=SysPath | name=Identifier)+ compileUnit								#Rmdisk
	| ('listdisks' | 'ldisks') ('-p' sys=SysPath)? compileUnit												#Ldisks

	| 'search' Integer par2=Identifier																		#Search


	| 'ls' files=F? dirs=D? path=(Identifier | Path)? compileUnit											#Ls
	| 'cd' path=Path? ident=Identifier? dots='..'? compileUnit												#Cd
	| 'mkdir' (id=Identifier | path=Path) compileUnit														#Mkdir
	| ('mk' | 'touch') (id=Identifier | path=Path) compileUnit												#MkFile
	| ('remove' | 'rm') (trgt=Path | id=Identifier) compileUnit												#Rm
	| ('rename' | 'rn') (id=Identifier | path=Path) dst=Identifier compileUnit								#Rn
	| ('move' | 'mv') src=(Path | Identifier) dst=(Path | Identifier) compileUnit							#Mv
	| ('copy' | 'cp') src=(Path | Identifier) dst=(Path | Identifier) compileUnit							#Cp
	| ('import' | 'im') ext=SysPath inte=(Path | Identifier) compileUnit									#Im
	| ('export' | 'ex') inte=(Path | Identifier) ext=SysPath compileUnit									#Ex

	| 'free'																								#Free
	| 'occ'																									#Occ
	| 'help'																								#Help
	| ('defrag' | 'df')																						#Defrag
	| ('exit' | 'quit' | 'q')																				#Exit
	;

optParams
	: ('-p' path=SysPath																								
	| '-n' name=Identifier
	| '-b' block=Integer)
	| '-pw' pw=Identifier
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
