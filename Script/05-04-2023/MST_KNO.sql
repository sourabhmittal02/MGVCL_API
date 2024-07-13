CREATE TABLE [dbo].[MST_KNO](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[USER_ID] [int] NULL,
	[KNO] [bigint] NULL,
	[IS_ACTIVE] [bit] NULL,
	[IS_DELETED] [bit] NULL,
	[TIME_STAMP] [datetime] NULL,
 CONSTRAINT [PK_MST_KNO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

SET IDENTITY_INSERT [dbo].[MST_KNO] ON 

INSERT [dbo].[MST_KNO] ([ID], [USER_ID], [KNO], [IS_ACTIVE], [IS_DELETED], [TIME_STAMP]) VALUES (1, 308, 1111, NULL, NULL, NULL)
INSERT [dbo].[MST_KNO] ([ID], [USER_ID], [KNO], [IS_ACTIVE], [IS_DELETED], [TIME_STAMP]) VALUES (2, 308, 111, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[MST_KNO] OFF