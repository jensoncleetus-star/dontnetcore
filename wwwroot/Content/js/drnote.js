var count = 1, type = '';
limits = 500;
//Add Row
function addrow(t, action, InvoiceNo, BillNo, InvoiceDate, SubTotal, TaxAmount, GrandTotal, CreditAmount, PaidAmount, BalanceAmount, TaxPer, CRGTotal, InvoiceCredits, SaleId, NoteType) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var optionunit = "";
        var required = "";
        var slno = $('#normalinvoice tr').length + 1;
        var a = "invoiceno" + count,
        tabindex = count * 5;
        var row = "<tr class='invoice_' id='invoice_" + count + "'>";
        var data = "";
        var price = 0;
        var baseprice = 0;
        var mrp = 0;
        var itemnote = "";
        var notbtn = "";
        var itemaddbtn = "";
        var divid = "invoiceno_" + InvoiceNo;

        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;

        if (InvoiceNo != null) {
            row = "<tr class='invoice_" + InvoiceNo + "' id='invoice_" + count + "'>";
            Option = "<option value='" + BillNo + "'>" + BillNo + "</option>";
        }
        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }
        SaleId = SaleId != null ? SaleId : 0;
        NoteType = NoteType != null ? NoteType : "";

        SubTotal = SubTotal != null ? SubTotal : 0.00;

        TaxAmount = TaxAmount != null ? TaxAmount : 0.00;

        GrandTotal = GrandTotal != null ? GrandTotal : 0.00;

        PaidAmount = PaidAmount != null ? PaidAmount : 0.00;
        TaxPer = TaxPer != null ? TaxPer : 0.00;

        CRGTotal = CRGTotal != null ? CRGTotal : 0.00;
        CreditAmount = CreditAmount != null ? CreditAmount : 0.00;
        InvoiceCredits = InvoiceCredits != null ? InvoiceCredits : 0.00;
        //CreditAmount = (CreditAmount - GTotal) || 0.00;
        var cramt = parseFloat(InvoiceCredits) - (parseFloat(CreditAmount) + parseFloat(TaxAmount));
        BalanceAmount = (parseFloat(GrandTotal) - (parseFloat(PaidAmount) + parseFloat(cramt))) || 0.00;

       // var cramt = (GTotal - TaxAmount) || 0.00;
        InvoiceDate = (InvoiceDate != "" && InvoiceDate != null) ? convertToDate(InvoiceDate) : "";

        data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td class='input-group input-group-sm' style='width: 100%;'><select class='form-control invoiceno' " + required + " data-id='" + count + "' placeholder='Item Name' data-msg-required='Invoice No is required' id='invoiceno_" + count + "'  data-val-required='Invoice No field is required' onchange='GetItemdetails(this," + count + ",\"" + type + "\")'>" + Option + "</select> " + itemaddbtn + "</td>" +
                "<td id='invoice_date_" + count + "' class='text-center'>" + InvoiceDate + "</td>" +
                "<td id='invoice_gtotal_" + count + "' class='text-center invoice_gtotal'>" + GrandTotal.toFixed(2) + "</td>" +
                "<td id='invoice_paid_" + count + "' class='text-center invoice_paid'>" + PaidAmount.toFixed(2) + "</td>" +
                "<td id='invoice_credit_" + count + "' class='text-center invoice_credit'>" + cramt.toFixed(2) + "</td>" +
                "<td id='invoice_balance_" + count + "' class='text-center invoice_balance'>" + BalanceAmount.toFixed(2) + "</td>" +
                
                "<td><input type='text' name='credit_amount[]' onchange='creditamount_change(" + count + ");' id='credit_amount_" + count + "' value='" + CreditAmount.toFixed(2) + "'  class='credit_amount_" + count + " form-control text-right credit_amount' placeholder='0' tabindex='" + tab2 + "' /><input type='hidden' id='salesentryid_" + count + "' value='" + SaleId + "'/><input type='hidden' id='NoteType_" + count + "' value='" + NoteType + "'/></td>" +
                "<td class='text-center'><button tabindex='" + tab3 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button>" + itemnote + "</td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        searchItem();

        //SubTotalCalculation();

        count++;
        setTabIndex();
    }
}
// search item
function searchItem() {
    var accId = $("#ddlpayto").val();
    if (accId != null) {
        $(".invoiceno").select2({
            placeholder: 'Search Invoice No',
            minimumInputLength: 0,
            ajax: {
                url: "/CreditSale/SearchInvoiceSale",
                dataType: 'json',
                type: "POST",
                delay: 50,
                data: function (params) {
                    return {
                        q: params.term,
                        page: params.page,
                        accId: accId,
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: data
                    };
                },
                cache: true
            },
        });
    }
}

function accbalance(accid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/ChekSale",
        data: JSON.stringify({ id: accid }),
        success: function (data) {
            var amount = data.balance.amount.toFixed(2) + " " + data.balance.type;
            $('#accdetails').text(amount);
            $('#accbalance').show();
            //if (data.data != null) {
            //    bindinvoice(data.data, receiptchk);
            //    $('.exp').hide();
            //    $('.sup').show();
            //}
            //else {
            //    $('#invoice').html('<hr/>');
            //    $('.exp').show();
            //    $('.sup').hide();
            //    grandTotal();
            //}
            //cashBalance();
        }
    });
}

function accbalanceTo(accid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/ChekSale",
        data: JSON.stringify({ id: accid }),
        success: function (data) {
            var amount = data.balance.amount.toFixed(2) + " " + data.balance.type;
            $('#Toaccdetails').text(amount);
            $('#Toaccbalance').show();
            //if (data.data != null) {
            //    bindinvoice(data.data, receiptchk);
            //    $('.exp').hide();
            //    $('.sup').show();
            //}
            //else {
            //    $('#invoice').html('<hr/>');
            //    $('.exp').show();
            //    $('.sup').hide();
            //    grandTotal();
            //}
            //cashBalance();
        }
    });
}


//Delete a row of table
function deleteRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'invoice_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        }
    }
    SubTotalCalculation();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}


function FindTaxNTotal() {
    var taxid = $("#taxss").val();
    var camount = $("#SubTotal").val();
    if (taxid != null) {
        $.ajax({
            async: true,
            cache: false,
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/Tax/ChekTax",
            data: JSON.stringify({ id: taxid }),
            success: function (data) {
                var tax = data.tax.toFixed(2);
                $('#TaxPer').val(tax);
                var taxamt = 0;
                if (tax != 0) {
                    taxamt = parseFloat(camount) * parseFloat((tax / 100));
                }
                var gtotal = parseFloat(camount) + parseFloat(taxamt);
                $('#TaxAmount').val(taxamt.toFixed(2));
                $('#GrandTotal').val(gtotal.toFixed(2));
               // AllItemTax();
            }
        });
    } else {
        $('#GrandTotal').val($("#CreditAmount").val());
    }   
}

function printNote(e) {
    $("#lblBillNo").text(e.summary.BillNo);
    $("#lblDate").text(convertToDate(e.summary.Date));
    $("#lblEmployee").text(e.summary.Cashier);
    if (e.summary.SExecutive == 0) {
        $("#executive").hide();
    }

    $("#lbltrn").text(e.summary.TRN);
    // bind Party details
    $("#lblParty").text(e.summary.PartyName);
    $("#lblAccNo").text(e.summary.PartyCode);
    var details = "";

    //Address
    if (e.summary.Address != null) {
        details += e.summary.Address;
    }

    //PO BOX
    if (e.summary.Zip != null) {
        if (e.summary.City == null) {
            details += " PO BOX : " + e.summary.Zip;
        }
        else {
            details += "<br /> PO BOX : " + e.summary.Zip;
        }

    }


    //State && Country
    if (e.summary.State != null && e.summary.Country != null) {
        if (e.summary.Zip != null) {
            details += ", " + e.summary.State + ", " + e.summary.Country;
        }
        else if (e.summary.Zip == null) {
            details += e.summary.State + ", " + e.summary.Country;
        }
    }

    //Phone

    if (e.summary.Phone != null && e.summary.Mobile != null) {
        details += "<br/> Phone " + e.summary.Phone; // + ", " + e.summary.Mobile;
    }
        //else if (e.summary.Phone == null && e.summary.Mobile != null) {
        //    details += "<br/> Phone " + e.summary.Mobile;
        //}
    else if (e.summary.Phone != null && e.summary.Mobile == null) {
        details += "<br/> Phone " + e.summary.Phone;
    }

    //Fax

    if (e.summary.Fax != null) {
        if (e.summary.Phone != null && e.summary.Mobile != null) {
            details += ", Fax  : " + e.summary.Fax;
        }
        else if (e.summary.Phone == null && e.summary.Mobile != null) {
            details += ", Fax  : " + e.summary.Fax;
        }
        else if (e.summary.Phone != null && e.summary.Mobile == null) {
            details += ", Fax  : " + e.summary.Fax;
        }
        else {
            details += "<br/>Fax  : " + e.summary.Fax;
        }
    }

    var str1 = "";
    var remark = "";
    var credit = "";
    if (e.summary.Note != null) {
        remark = e.summary.Note.replace(/\n/g, "<br/>");
    }


    // bind items
    if (e.summary.ReturnType == 0) {//aginst
        var itemsData = bindItem(e);
        $('#itemtable').append(itemsData);
    } else {
        var itemsData = bindItemDirect(e);
        $('#itemtable').append(itemsData);
    }

    var grt = parseFloat(e.summary.GrandTotal).toFixed(2);
    var word = conNumber(grt);

    var Remarks = "";
    var Rem = e.summary.Remarks != "" ? e.summary.Remarks.replace(/\n/g, "<br/>") : "";
    if (e.summary.Remarks != "") {
        Remarks += "<tr class='border-top'><td colspan='2' style='height: 50px;'><strong><u>Remarks :</u></strong><br/>" + Rem + " </td></tr>";
    }
    if (e.summary.Remarks == "") {
        Remarks += "";
    }

    credit += "<tr class='border-top'><td style='width:30%;'>Credit Amount in AED</td><td style='width:70%;text-align: right;'>" + parseFloat(e.summary.CreditAmount).toFixed(2) + "</td></tr>";
    if (e.summary.TaxAmount > 0) {
        credit += "<tr class='border-top'><td style='width:30%;'>Total Tax Amount in AED (" + e.summary.TaxPer + " %)</td><td style='width:70%;text-align: right;'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
    }
    credit += "<tr class='border-top'><td style='width:30%;'>Grand Total in AED</td><td style='width:70%;text-align: right;'>" + grt + "<br/><strong>Credit " + word + " Only</strong></td></tr>";
    tandc = "<tr class='border-top'><td colspan='2' style='height: 50px;'><strong><u>Terms And Conditions :</u></strong><br/>" + remark + " </td></tr>";
    str1 = credit + Remarks + tandc;


    $('#itemtable1').append(str1);
    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    var titname = "Tax Credit Invoice - " + e.summary.PartyName + " - " + e.summary.BillNo;
    $('title').html(titname);
    // find height

    var header = $(".print thead").height(); // default 265
    var items = $("#itemSection").height(); // default 558
    var itemstable = $("#itemtable").height();
    var terms = $("#itemtable1").height(); // default 137
    var footer = $("#footer").height(); // default 50
    var height = $(".print").height(); // total
    if (terms > 137 && itemstable < 558) {
        //$('#container').css('min-height', '360px');
        //$('#container').attr('style','min-height:360px;other-styles');
    }
    window.print();
}


//itembind
function bindItem(e) {
    var total = parseFloat(0);
    var str = "";
    var count = 1;
    $.each(e.item, function (i, item) {
        str += '<tr>';
        str += '<td>' + count + '</td>';
        str += '<td>' + item.BillNo + '</td>';
        str += '<td>' + convertToDate(item.Date) + '</td>';
        str += '<td>' + item.CreditAmount + '</td>';
        str += '</tr>';
        count++;
    });
    return str;
}

//itembind
function bindItemDirect(e) {
    var total = parseFloat(0);
    var str = "";
    var count = 1;
    str += '<tr>';
    str += '<td>' + count + '</td>';
    str += '<td>' + e.summary.description + '</td>';
    str += '<td>' + convertToDate(e.summary.Date) + '</td>';
    str += '<td>' + e.summary.CreditAmount + '</td>';
    str += '</tr>';
    return str;
}


function directBalance(custId) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/ChekCreditNote",
        data: JSON.stringify({ id: custId }),
        success: function (data) {
            var amount = data.balance.amount.toFixed(2);
            $("#BalanceAmount").val(amount + " : " + data.balance.type);
        }
    });
}


function setTabIndex() {
    var j = 1;
    $('body').find('input,textarea,select,button, .select2-container .select2-selection__rendered').not(".select2-hidden-accessible").not(":hidden").each(function (i) {
        if (!$(this).hasClass("select2-hidden-accessible") && !$(this).is(":hidden")) {
            $(this).attr('tabindex', j);
            j++;
        }
        if ($(this).closest("tr").hasClass("invoice_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}

function taxpopup() {
    $('#modal-tax').on('submit', '#createform', function (e) {
        var url = $('#modal-tax #createform')[0].action;
        var text = $("#TaxName").val();
        $('#taxss option:selected').attr("selected", null);
        $.ajax({
            type: "POST",
            url: url,
            data: $('#modal-tax #createform').serialize(),
            success: function (data) {
                if (data.status) {
                    $('#modal-tax').modal('hide');

                    var newOption = $('<option></option>');
                    newOption.val(data.Id).attr("selected", "selected");
                    newOption.html(text);
                    $('#taxss').append(newOption);
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                }
                fadeAlert();
            }
        })

        e.preventDefault();
    })

    $('body').on('click', '.modal-close-btn', function () {
        $('#modal-tax').modal('hide');
        $('#modal-tax').removeData('bs.modal');
    });
}

function GetItemdetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".invoice_" + ItemId).length > 0) {
                alert("Sorry You Cant Add An Invoice More Than 1 Times");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }
}

// update item details
function itemUpdate(selectObject, dataid, action) {

    $.ajax({
        url: '/CreditSale/GetSalesById',
        type: "GET",
        dataType: "JSON",
        data: { billNo: selectObject.value },
        success: function (result) {
            $("#invoice_gtotal_" + dataid).text(result.GrandTotal.toFixed(2));
            $("#invoice_paid_" + dataid).text(result.PaidAmount.toFixed(2));
            $("#invoice_balance_" + dataid).text(result.BalanceAmt.toFixed(2));
            //$("#invoice_credit_" + dataid).text(result.CreditAmount.toFixed(2));
            //// $("#invoice_subtotal_" + dataid).text(result.SubTotal.toFixed(2));
            //// $("#invoice_taxamount_" + dataid).text(result.TaxAmount.toFixed(2));
            $("#invoice_date_" + dataid).text(convertToDate(result.InvoiceDate));
            $("#salesentryid_" + dataid).val(result.SeEntry);
            $("#NoteType_" + dataid).val(result.NoteType);
            

            var cramt = result.CreditAmount!=null?result.CreditAmount:0.00;
            $("#invoice_credit_" + dataid).text(cramt.toFixed(2));

            
            $("#credit_amount_" + dataid).val("0.00");

            $(selectObject).closest('tr').attr('class', "invoice_" + result.InvoiceNo);

            if ($(".invoice_").length == 0) {
                addrow('invoiceList', '', '', '', '');
            }
        }
    });
}

// generate Price Table
function InvoiceListTable(fnval) {
    $("#fnvalp").val(fnval);
    addrow('invoiceList', '', '', '', '');
    $("#modal-invoiceList").modal({ show: true, backdrop: "static" });
}

function creditamount_change(arg) {
    var creditamt = $("#invoice_" + arg + " .credit_amount").val();
    //var subtotal = $("#invoice_" + arg + " .invoice_subtotal").text();
    var balamt = $("#invoice_" + arg + " .invoice_balance").text();

    //Separate Tax
    //var tax = $('#TaxPer').val();
    //$("#invoice_" + arg + " .tax_per").val(tax);
    //var taxamt = parseFloat(creditamt) * parseFloat((tax / 100));
    var gtotal = parseFloat(creditamt); //+ parseFloat(taxamt);
    if (gtotal > balamt) {
        alert("Credit Amount Cannot be Greater than Balance Amount..");
        $('.credit_amount').val('0.00');
        $('.credit_amount').text('0.00');
    }
    else {
        $("#invoice_" + arg + " .grand_total").val(gtotal.toFixed(2));
        //$("#invoice_" + arg + " .tax_amount").val(taxamt.toFixed(2));
    }

    //SubTotalCalculation();


    //if (parseFloat(creditamt) > parseFloat(balamt)) {
    //    $("#invoice_" + arg + " .credit_amount").val("0.00");
    //    alert("Credit Amount should Less Than or Equal to Grand Total..!");

    //    //SubTotalCalculation();
    //} else {
    //    //SubTotalCalculation();
    //}
    //FindTaxNTotal();
}
function SubTotalCalculation()
{
    var tbody = $("#cninvoiceList tbody");
    if (tbody.children().length > 0) {
            var CAmt = 0;
            $(".credit_amount").each(function () {
                var subtax = $(this).val();
                subtax = subtax || 0;
                CAmt = parseFloat(CAmt) + parseFloat(subtax);
            });
            $("#SubTotal").val(parseFloat(CAmt).toFixed(2));
            FindTaxNTotal();
    }
}

function PrintInvoice(data) {
    $("[id$=lblBillNo]").text(data.VoucherNo);
    $("[id$=lblDate]").text(convertToDate(data.Date));

    $("[id$=lblaccountsF]").text(data.PayFrom);
    $("[id$=lblaccountsT]").text(data.PayTo);

    $("[id$=lblcreditamt]").text(data.CrAmt.toFixed(2));

    $("[id$=lblcreditsum]").text(data.CrAmt.toFixed(2));


    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('body').html(printContent);
    window.print();
}

function getEmailId() {
    var AccId = $('#ddlpayto').val();
    if (AccId != "") {
        $.ajax({
            url: '/Accounts/GetEmailByIdByAccount',
            type: "GET",
            dataType: "JSON",
            data: { AccId: AccId },
            success: function (result) {
                $("#suppEmailId").val(result.EmailId);
            }
        });
    }
}