CREATE PROCEDURE [dbo].[getSalesLedgerByDateRange](
    @startDate AS Date = Null
    ,@endDate AS Date = Null
	,@customerId AS int
)
AS
if(@startDate !='' and @endDate != '')
BEGIN
select Date,sinvoice.InvoiceId, Particulars, DebitAmount,CreditAmount,BalanceAmount from SalesLedger sl
join SalesInvoice sinvoice on sinvoice.Id = sl.InvoiceId
where sinvoice.CustomerId =@customerId and (Date  between @startDate And @endDate)
END
Else
BEGIN
select Date,sinvoice.InvoiceId, Particulars, DebitAmount,CreditAmount,BalanceAmount from SalesLedger sl
join SalesInvoice sinvoice on sinvoice.Id = sl.InvoiceId
where sinvoice.CustomerId = @customerId
END
GO


select * from SalesInvoice
select * from salesLedger