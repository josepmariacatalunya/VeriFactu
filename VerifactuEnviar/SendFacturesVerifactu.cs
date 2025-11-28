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
        var invoice = new Invoice("GIT-EJ-0002", new DateTime(2024, 11, 15), "B72877814")
        {
            InvoiceType = TipoFactura.F1,
            SellerName = "WEFINZ GANDIA SL",
            BuyerID = "B44531218",
            BuyerName = "WEFINZ SOLUTIONS SL",
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
            TaxBase = 100,
            TaxAmount = 21
        }
    }
        };

        try
        {

            // Creamos la entrada de la factura
            var invoiceEntry = new InvoiceEntry(invoice);
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }


}
