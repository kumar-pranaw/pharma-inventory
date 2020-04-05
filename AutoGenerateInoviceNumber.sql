CREATE TABLE SalesInvoice(
	[ID] [int] IDENTITY(1,1) Primary Key,
	[InvoiceId]  AS ('NERP'+right('00000'+CONVERT([varchar](4),[ID]),(4))) PERSISTED,
	[CustomerId] [int] references Customer(id),
	[DateOfPurchase] [date] NULL,
	[TotalPurchaseAmount] [decimal](18, 0) NULL,
)