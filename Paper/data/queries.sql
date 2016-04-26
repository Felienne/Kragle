--1 size metrics
select projectID, count(distinct spriteName) as spritesWithCodeNo, max(codeBlockRank) + 1 as codeBlocksNo, count(*) as linesNo, max(intent) as maxIndentation
FROM code
group by projectID

--2 lines per sprite type
select projectID, spriteType, count(*) as linesNo
FROM code
group by projectID, spriteType

--3 cyclomatic complexity
select projectID, codeBlockRank, count(*) + 1 from code
where command in ('doifelse','doif')
group by projectID, codeBlockRank
--cyclomatic complexity of procedures
select projectID, codeBlockRank, count(*) + 1 from code
where command in ('doifelse','doif')
and spriteType = 'procdef'
group by projectID, codeBlockRank

--4 projects with functions per spriteType
select a.projectID, a.spriteType, count(*) as blocksNo
FROM code a
where exists (select * from code c where c.projectID = a.projectID and c.spriteType = 'procdef')
group by a.projectID, a.spriteType
--total blocks in projects with functions
select count(*) as blocksNo
FROM code a
where exists (select * from code c where c.projectID = a.projectID and c.spriteType = 'procdef')
--blocks in functions
select count(*) as linesNo
FROM code a
where a.spriteType = 'procdef'

--create function
USE [Kragle]
GO
/****** Object:  UserDefinedFunction [dbo].[CountOccurancesOfString]    Script Date: 4/19/2016 11:33:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[CountOccurancesOfString]
(
    @searchString nvarchar(max),
    @searchTerm nvarchar(max)
)
RETURNS INT
AS
BEGIN
    return (LEN(@searchString)-LEN(REPLACE(@searchString,@searchTerm,'')))/LEN(@searchTerm)
END

--5 procedure arguments
select distinct projectID, codeBlockRank, REPLACE(spriteName, ',','(comma)'), 
dbo.CountOccurancesOfString(spritename, ' %n') as argsNumber, 
dbo.CountOccurancesOfString(spritename, ' %s') as argsText, 
dbo.CountOccurancesOfString(spritename, ' %b') as argsBoolean
from code
where [spriteType] = 'procdef'

--6 procedures per project
select  projectID, count(distinct codeBlockRank)
from code
where [spriteType] = 'procdef'
group by projectID

--7 function calls
SELECT projectid, Replace(spriteName,',','') , replace(param1,',',''), count(distinct codeBlockRank) as codeBlocks, count(*) as calls
from code
where command  = 'call'
group by projectid, spriteName, param1
UNION
select distinct projectid, Replace(spriteName,',','') , 'not used', 0, 0
from code a
where a.spriteType = 'procdef'
and not exists (select * from code c
where c.command  = 'call' and c.param1 = a.spriteName)
--recursion
(SELECT count(*)
from code
where command  = 'call'
and spriteType = 'procdef')
(SELECT count(*)
from code
where command  = 'call')
(SELECT count(*)
from code
where command  = 'call'
and spriteType = 'procdef'
and param1 = spriteName)

--8 variablesperproject
select projectid, count(distinct param1)
from code
where command in ('readVariable', 'setVar:to:', 'showVariable:', 'changeVar:by:', 'hideVariable:') 
group by projectid
--list related blocks
--'contentsOfList:', '1getLine:ofList:', 'lineCountOfList:', 'append:toList:', 'deleteLine:ofList:', 'insert:at:ofList:', 'setLine:ofList:to:', 'list:contains:', 'showList:', 'hideList:'
--variables not initialzed
select distinct c.projectid, c.param1
from code c
where c.command in ('readVariable', 'showVariable:', 'changeVar:by:', 'hideVariable:') 
and not exists(select * from code d where c.projectID = d.projectID and c.param1 = d.param1 and d.command = 'setVar:to:')
--code blocks
select projectid, param1, count(distinct codeBlockRank)
from code
where command in ('readVariable', 'setVar:to:', 'showVariable:', 'changeVar:by:', 'hideVariable:') 
group by projectid, param1

--9 conditional statements
select projectid, count(*)
from code
where command in ('doIf', 'doIfElse') 
group by projectid
--loops
select projectid, count(*)
from code
where command in ('doRepeat', 'doForever', 'doUntil') 
group by projectid

--10 events
select projectid, count(*)
from code
where command in ('broadcast:', 'doBroadcastAndWait', 'whenIReceive') 
group by projectid
--receive blocks without broadcast
select *
from code a
where a.command = 'whenIReceive'
and not exists (select * from code b where a.projectID = b.projectID and a.param1 = b.param1 and b.command in ('broadcast:', 'doBroadcastAndWait'))
(select distinct projectID
from code a
where a.command in ('broadcast:', 'doBroadcastAndWait')
and not exists (select * from code b where a.projectID = b.projectID and a.param1 = b.param1 and b.command = 'whenIReceive')
)

--11 clones
--create tables, flatten blocks
--without variables
Select distinct ST2.projectID, ST2.spriteType, ST2.spriteName, ST2.codeBlockRank, count(*) as lines,
    substring(
        (
            Select ','+ST1.command  AS [text()]
            From dbo.code ST1
            Where ST1.projectID = ST2.projectID and ST1.codeBlockRank = ST2.codeBlockRank
            ORDER BY ST1.codeBlockRank, St1.line
            For XML PATH ('')
        ), 2, 1000) [code]
INTO clonesWithoutVariable
From dbo.code ST2
group by ST2.projectID, ST2.spriteType, ST2.spriteName, ST2.codeBlockRank
order by codeBlockRank
--with variables
Select distinct ST2.projectID, ST2.spriteType, ST2.spriteName, ST2.codeBlockRank, count(*) as lines,
    substring(
        (
            Select ','+ST1.command+' '+ST1.param1+' '+ST1.param2+' '+ST1.param3+' '+ST1.param4+' '+ST1.param5+' '+ST1.param6  AS [text()]
            From dbo.code ST1
            Where ST1.projectID = ST2.projectID and ST1.codeBlockRank = ST2.codeBlockRank
            ORDER BY ST1.codeBlockRank, St1.line
            For XML PATH ('')
        ), 2, 1000) [code]
INTO clonesWithVariables
From dbo.code ST2
group by ST2.projectID, ST2.spriteType, ST2.spriteName, ST2.codeBlockRank
order by codeBlockRank
--without the first event
Select distinct ST2.projectID, ST2.spriteType, ST2.spriteName, ST2.codeBlockRank, count(*) as lines,
    substring(
        (
            Select ','+ST1.command  AS [text()]
            From dbo.code ST1
            Where ST1.projectID = ST2.projectID and ST1.codeBlockRank = ST2.codeBlockRank
			and line > 0
            ORDER BY ST1.codeBlockRank, St1.line
            For XML PATH ('')
        ), 2, 1000) [code]
INTO clonesWithoutEvent
From dbo.code ST2
group by ST2.projectID, ST2.spriteType, ST2.spriteName, ST2.codeBlockRank
order by codeBlockRank
  --select without variables, whole blocks, accross sprites
  select a.lines, count(*)
  from
  (select projectid, spriteType, lines, code, count(distinct spriteName) as differentSprites, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutVariable]
  group by projectid, spriteType, lines, code
  having count(*) >1) a
  group by a.lines
  --without variables, whole blocks, same sprite
  select a.lines, count(*)
  from
  (select projectid, spriteType, spriteName, lines, code, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutVariable]
  where spriteType != 'procDef'
  group by projectid, spriteType, spriteName, lines, code
  having count(*) >1) a
  group by a.lines
--for all three
  delete from [Kragle].[dbo].[clonesWithVariables]
  where lines < 5
 --without variables, same sprites
  select a.projectid, count(*)
  from
  (select projectid, spriteType, spriteName, lines, code, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutVariable]
  where spriteType != 'procDef'
  group by projectid, spriteType, spriteName, lines, code
  having count(*) >1) a
  group by a.projectid
--accrosss spites
  select a.projectid, count(*)
  from
  (select projectid, spriteType, lines, code, count(*) as clonesNo
  FROM [Kragle].[dbo].clonesWithVariables
  group by projectid, spriteType, lines, code
  having count(*) >1) a
  group by a.projectid
--cloned procedures
  select a.projectid, count(*)
  from
  (select projectid, spriteType, lines, code, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutVariable]
  where spriteType = 'procDef'
  group by projectid, spriteType, lines, code
  having count(*) >1) a
  group by a.projectid
--clonescopies
select projectid, spriteType, lines, count(*) as clonesNo
  FROM [Kragle].[dbo].clonesWithVariables
  group by projectid, spriteType, lines, code
  having count(*) >1
  select projectid, spriteType, lines, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutVariable]
  where spriteType != 'procDef'
  group by projectid, spriteType, spriteName, lines, code
  having count(*) >1
---functionality blocks cloned
(
  select projectid, minCodeBlock
  from
  (select projectid, spriteType, lines, code, min(codeblockRank) as minCodeBlock, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutVariable]
  group by projectid, spriteType, lines, code
  having count(*) >1) as a
  )
  except
  (
  select projectid, minCodeBlock
  from
  (select projectid, spriteType, lines, code, min(codeblockrank) as minCodeBlock, count(*) as clonesNo
  FROM [Kragle].[dbo].[clonesWithoutEvent]
  group by projectid, spriteType, lines, code
  having count(*) >1) as a
  )
---accross projects
  select lines, code, count(*) as clonesNo, count(distinct projectid) as distinctProjects
  FROM [Kragle].[dbo].clonesWithoutVariable
  group by lines, code
  having count(*) >100
  order by distinctProjects desc

--12 dead code
select projectid, count(*)
from code
where (spriteType!='procdef' and line = 0 and totalLines = 1)
or
(
spriteType!='procdef'
and line = 0
and command not in ('whenCloned',
'whenGreenFlag',
'whenKeyPressed',
'whenClicked',
'whenSceneStarts',
'whenSensorGreaterThan',
'whenIReceive',
'LEGOWeDo\u001fwhenDistance',
'LEGOWeDo\u001fwhenTilt',
'LEGOWeDo2.0\u001fwhenDistance',
'LEGOWeDo2.0\u001fwhenTilted',
'PicoBoard\u001fwhenSensorConnected',
'PicoBoard\u001fwhenSensorPass')
)
group by projectID


--13 large script, sprite
select totalLines, count(*)
from code
where line = 0
group by totalLines
---blocks in sprites
select a.totalLinesInSprite, count(*)
from
(select projectID, spriteName, sum(totalLines) as totalLinesInSprite
from code
where line = 0
group by projectID, spriteName)  a
group by a.totalLinesInSprite
--maximum lines per project
select projectid, max(totalLines)
from code
group by projectID
--maximum lines per sprite
select a.projectID, max(a.totalLinesInSprite)
from
(select projectID, spriteName, sum(totalLines) as totalLinesInSprite
from code
where line = 0
group by projectID, spriteName)  a
group by a.projectID

--15 interactivity
select projectID, count(*) as inputblocks
from code, blockTypes
where command = block
and [IsInput?] = 'Yes'
group by projectID
--populatiry of commands
select command, count(distinct projectid), count(*) as inputblocks
from code, blockTypes
where command = block
and [IsInput?] = 'Yes'
group by command
--which key in whenKeypressed
select param1, count(*)
from code
where command = 'whenKeyPressed'
group by param1

--16 categories of blocks
select [Category], count(*)
from code, blocktypes
where code.command = blocktypes.Block
group by category
--projects with Pen block
select category,count(*), count(distinct projectID)
from code, blocktypes
where code.command = blocktypes.Block
and category = 'Pen'
group by Category