using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Business;
using VeriFactu.Xml.Factu.Alta;


public class SendFacturesVerifactu
{
    public bool EnviarFactura()
    {
        // Creamos una instacia de la clase factura
        var invoice = new Invoice("GIT-EJ-0016", new DateTime(2025, 11, 16), "43710521X")
        {
            InvoiceType = TipoFactura.F1,
            SellerName = "JOSEP MARIA MASAGUE ESCODA",
            BuyerID = "B25721549",
            BuyerName = "MASAGUE GESTIO SL SL",
            Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
            TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxRate = 21,
            TaxBase = 200,
            TaxAmount = 42
        }
    }
        };

        try
        {

            // Creamos la entrada de la factura
            var invoiceEntry = new InvoiceEntry(invoice);
            invoiceEntry.Save();
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }


}
