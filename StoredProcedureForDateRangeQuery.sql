Alter PROCEDURE getLedgerByDateRange(
    @startDate AS Date = Null
    ,@endDate AS Date = Null
	,@supplierId AS int
)
AS
if(@startDate !='' and @endDate != '')
BEGIN
select Date,pin.InvoiceId, Particulars, DebitAmunt,CreditAmount,BalanceAmount from PurchaseLedger pl
join PurchaseInvoice pin on pin.Id = pl.InvoiceId
where pin.SupplierId =@supplierId and (Date  between @startDate And @endDate)
END
Else
BEGIN
select Date,pin.InvoiceId, Particulars, DebitAmunt,CreditAmount,BalanceAmount from PurchaseLedger pl
join PurchaseInvoice pin on pin.Id = pl.InvoiceId
where pin.SupplierId =@supplierId
END