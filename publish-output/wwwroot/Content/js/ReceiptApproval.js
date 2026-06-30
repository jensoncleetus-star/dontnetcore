// function for mode changing
function modechange() {
    var mop = $('#MoPay').val();
    var pdate = $('#PDCDate').val();
    if (mop == 0 || mop == 3) {
        $("#pcdfor").text('');
        $('#PDCDate').val('');
        $('#PDCDate').prop('disabled', true);
        $('#PDCDate').prop('required', false);
        $('#checkno').hide();
        $('#bank').hide();
        $('#PDCDate').attr('tabindex', -1);
    }
    else {
        datepickerInit();
        $("#pcdfor").text('*');
        if (pdate == '') {
            $('#PDCDate').val(today());
        }
        $('#PDCDate').prop('disabled', false);
        $('#PDCDate').prop('required', true);
        $('#checkno').show();
        $('#bank').show();
        var tab = $('#MoPay').attr('tabindex');
        tab++;
        $('#PDCDate').attr('tabindex', tab);
    }

    if ($('#chkPdc').val() == "Yes" && mop == 1) {
        $('#PDCDate').prop('disabled', true);
        $('#PDCDate').val($("#PdcDate").val());
    }
}
// find account balance on select
function accbalance(accid, receiptchk) {
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
            if (data.data != null) {
                //bindinvoice(data.data, receiptchk);
                bindinvoiceNew();

                $('.exp').hide();
                $('.sup').show();
            }
            else {
                $('#invoice').html('<hr/>');
                $('.exp').show();
                $('.sup').hide();
                grandTotal();
            }
            cashBalance();
        }
    });
}
function accbalanceto(accid) {
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
            $('#accdetailsto').text(amount);
            $('#accbalanceto').show();
        }
    });
}
// bind supplier unpaid invoice details in table

function bindinvoiceNew() {
    var table = '<table class="table table-bordered table-hover text-center" id="invoicedata">' +
        '<thead>' +
        '<tr class="bg-gray">' +
        '<th class="text-center">#</th><th class="text-center">Type of Ref</th>' +
        '<th id="headinvoice" class="text-center">Invoice No / Name</th>' +
        //'<th class="text-center">Date</th><th class="text-center">Balance</th>' +
        '<th class="text-center">Amount</th>' +
        '<th class="text-center">Action</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody id="addinvoiceItem"></tbody></table>';
    $('#invoice').html(table);

    addrows('addinvoiceItem', 'new', '', '', '', '0.00', '0.00', '', '');
}
var count = 1, type = '';
limits = 500; var Amt = 0;
function addrows(t, action, Invoice, BillNo, InvoiceDate, Balance, Amount, Type, NewRefName, RType) {

    tabindex = count * 5;
    var slno = $('#addinvoiceItem tr').length + 1;
    var row = "<tr class='invoice_' id='invoice_" + count + "'>";
    var divid = "invoice_name_" + Invoice;
    tab1 = tabindex + 1;
    tab2 = tabindex + 2;
    tab3 = tabindex + 3;
    tab4 = tabindex + 4;
    tab5 = tabindex + 5;

    var hide = "";
    if (action == "new") {
        initialload();
        hide = "hidden";
    }

    var rowOne = "";
    var rowTwo = "";
    var rowread = "";
    var row1 = "";
    var row2 = "";
    var row3 = "";
    var row4 = "";

    if (RType != null) {
        row = "<tr class='invoice_" + count + "' id='invoice_" + count + "'>";
    }

    if (action == "edit") {
        if (RType == "New Reference") {
            rowOne = "hidden";
            row1 = "Selected";
        } else if (RType == "Against Reference") {
            rowTwo = "hidden";
            row2 = "Selected";
        } else if (RType == "Advance") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row3 = "Selected";
        } else if (RType == "On Account") {
            rowOne = "hidden";
            rowread = "readonly='readonly'";
            NewRefName = "";
            row4 = "Selected";
        } else {
            rowOne = "hidden";
            NewRefName = "";
            row1 = "Selected";
        }
    }

    var Option = "";
    if (Invoice != null) {
        Option = "<option value='" + Invoice + "'>" + BillNo + "</option>";
    }

    var OptionType = "<option value='New Reference' " + row1 + ">New Reference</option>" +
        "<option value='Against Reference' " + row2 + ">Against Reference</option>" +
        "<option value='Advance' " + row3 + ">Advance</option>" +
        "<option value='On Account' " + row4 + ">On Account</option>";

    data = "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
        "<td id='td_type_" + count + "' class='input-group input-group-sm td_type' width='100%'><select data-name='Type' class='form-control type_name' data-id='" + count + "' placeholder='Select Type' id='type_name_" + count + "' onchange='GetTypeChange(this," + count + ",\"" + action + "\")'>" + OptionType + "</select></td>" +
        "<td " + hide + rowOne + " id='td_invoice_" + count + "' class='td_invoice'><select data-name='InvoiceNo' class='form-control invoice_name' data-id='" + count + "' placeholder='Select Invoice' id='invoice_name_" + count + "' onchange='GetInvoiceDetails(this," + count + ",\"" + action + "\")'>" + Option + "</select></td>" +
        //"<td><input type='text' data-name='' id='invoice_date_" + count + "' value='" + InvoiceDate + "'  class='invoice_date_" + count + " form-control text-center' tabindex='" + tab2 + "' readonly='readonly' /></td>" +
        "<td " + rowTwo + " id='td_refname_" + count + "' class='td_refname'><input type='text' " + rowread + " data-name='NewRefName' id='newrefname_" + count + "' value='" + NewRefName + "'  class='newrefname_" + count + " form-control text-center' tabindex='" + tab2 + "' /></td>" +
        "<td><input type='number' data-name='Amount' onchange='invoice_amt_change(" + count + ");' id='invoice_amt_" + count + "' value='" + parseFloat(Amount).toFixed(2) + "'  class='invoice_amt_" + count + " form-control text-center invamt' placeholder='0' min='0' tabindex='" + tab4 + "' /></td>" +
        "<td><button type='button' tabindex='" + tab5 + "' style='text-align: right;' class='btn btn-danger'  value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button> " +
        "<input type='hidden' data-name='BillType'  class='invoice_type_" + count + "' id='invoice_type_" + count + "' value='" + Type + "'/>" +
        "<input type='hidden' data-name='' id='invoice_balance_" + count + "' value='" + parseFloat(Balance).toFixed(2) + "'  class='invoice_balance_" + count + " ' />" +
        //"<input type='hidden' data-name='' class='invoice_balanceamt_" + count + "' id='invoice_balanceamt_" + count + "' value='" + BalAmt + "'/>"+
        "</td>";
    row += data + "</tr>";
    $('#' + t).append(row);
    searchInvoice();

    count++;
    setTabIndex();

    Amt += parseFloat(Amount);
    if (Amt > 0) {
        $("#Paying").prop('disabled', true);
    }



}
function searchInvoice() {
    var selecteditem = new Array();
    $(".invoice_name").each(function () {
        selecteditem.push($(this).val());
    });

    var account = $("#ddlpayfrom").val();

    $(".invoice_name").select2({
        placeholder: 'Search Invoice',
        minimumInputLength: 0,
        ajax: {
            url: "/Accounts/SearchAccountsByIdSelect",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 1,
                    account: account
                };
            },
            processResults: function (data, params) {
                // parse the results into the format expected by Select2
                // since we are using custom formatting functions we do not need to
                // alter the remote JSON data, except to indicate that infinite
                // scrolling can be used
                params.page = params.page || 1;
                return {
                    results: data,
                    pagination: {
                        //more: (params.page * 10) < 50
                        more: true
                    }
                };
            },
            cache: true
        },
        templateResult: repoFormatResult,
        templateSelection: repoFormatSelection,
    });
}

function repoFormatResult(repo) {
    var markup = '<div class="se-row">' +
        '<h4>' + repo.text + '</h4>';
    if (repo.Date != null) {
        markup += '<div class="se-sec">  Date  : ' + convertToDate(repo.Date) + '</div>';
    }

    markup += '<div class="se-sec">Amount  : ' + repo.Amount + '</div>';
    markup += '<div class="se-sec">Balance : ' + repo.Balance + '</div>';
    markup += '</div>';
    var retn = $(markup);
    return retn;
}

function repoFormatSelection(repo) {
    return repo.text;
}

function GetTypeChange(selectObject, dataid, action) {

    //if (action == "new") {
    if (selectObject.value == "New Reference") {

        $(".td_invoice_" + dataid).hide();
        $(".td_refname_" + dataid).show();

        $("#td_invoice_" + dataid).hide();
        $("#td_refname_" + dataid).show();

        $("#newrefname_" + dataid).prop('disabled', false);

    } else if (selectObject.value == "Against Reference") {


        $(".td_invoice_" + dataid).show();
        $(".td_refname_" + dataid).hide();

        $("#td_invoice_" + dataid).show();
        $("#td_refname_" + dataid).hide();

        //$("#newrefname_" + dataid).prop('disabled', false);


    } else if (selectObject.value == "Advance") {


        $(".td_invoice_" + dataid).hide();
        $(".td_refname_" + dataid).hide();

        $("#td_invoice_" + dataid).hide();
        $("#td_refname_" + dataid).show();

        $("#newrefname_" + dataid).prop('disabled', true);
    }
    else if (selectObject.value == "On Account") {

        $(".td_invoice_" + dataid).hide();
        $(".td_refname_" + dataid).hide();

        $("#td_invoice_" + dataid).hide();
        $("#td_refname_" + dataid).show();

        $("#newrefname_" + dataid).prop('disabled', true);

    }
    //}
}

function initialload() {
    $("#td_invoice").hide();
    $("#td_refname").show();

    $("#td_headnull").hide();
}
function GetInvoiceDetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var ItemId = selectObject.value;
        if (ItemId != null) {
            if ($(".invoice_" + ItemId).length > 0) {
                alert("Sorry You Cant Add An Item More Than One Time");
                $(selectObject).val(null).trigger('change');
            }
            else {
                itemUpdate(selectObject, dataid, action);
            }
        }
    }
}

function itemUpdate(selectObject, dataid, action) {
    var entry = "";
    var url = "";
    var payfrom = $("#ddlpayfrom").val();
    //if (action == "edit") {
    //    url = '/ReceiptApproval/GetReceiptBill';
    //    sentry = getQueryString('');
    //} else {
    url = '/Accounts/SearchAccountsById';
    entry = selectObject.value;
    //}

    $.ajax({
        url: url,
        dataType: 'json',
        data: { account: payfrom, entry: entry },
        cache: true,
        success: function (data) {

            $("#newrefname_" + dataid).val(data.text);


            $("#invoice_balance_" + dataid).val(data.Balance);
            if (data.Date != null) {
                $("#invoice_date_" + dataid).val(convertToDate(data.Date));
            }
            $("#invoice_balance_" + dataid).val(parseFloat(data.Balance).toFixed(2));

            //var amt = data.Balance < data.Amount ? data.Balance : data.Amount;
            if (action == "edit") {
                $("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));
            } else {
                $("#invoice_amt_" + dataid).val(parseFloat(data.Balance).toFixed(2));
            }

            if (data.Amount > 0) {
                $("#Paying").prop('disabled', true);
            } else {
                $("#Paying").prop('disabled', false);
            }

            //$("#invoice_amt_" + dataid).val(parseFloat(data.Amount).toFixed(2));

            $("#invoice_type_" + dataid).val(data.type);
            // $("#invoice_balanceamt_" + dataid).val(data.BalAmt);

            $(selectObject).closest('tr').attr('class', "invoice_" + data.id);

            if (data.id != null && data.Amount > 0) {
                var count = 0;
                $("#addinvoiceItem tr").each(function () {
                    var classname = $(this).closest('tr').attr('class');
                    if (classname == 'invoice_') {
                        count++;
                    }
                });
                if (count == 0)
                    addrows('addinvoiceItem', 'new', '', '', '', '0.00', '0.00', '', '');
            }
            RowTotal();
        }
    });
}

function invoice_amt_change(arg) {
    var bal = $("#invoice_balance_" + arg).val();
    var amt = $("#invoice_amt_" + arg).val();
    var type = $("#type_name_" + arg).val();

    if (type == "Against Reference") {
        if (bal != 0 && parseFloat(bal) < parseFloat(amt)) {
            alert("Amount Should Less than or Equals to Balance Amount..!!");
            $("#invoice_amt_" + arg).val(parseFloat(bal).toFixed(2));
        }
    }

    if (parseFloat(amt) > 0) {
        $("#Paying").prop('disabled', true);
    } else {
        $("#Paying").prop('disabled', false);
    }
    $("#invoice_amt_" + arg).closest('tr').attr('class', "invoice_" + arg);


    //--------check empty rows----------------------------------------
    var count = 0;
    $("#addinvoiceItem tr").each(function () {
        var classname = $(this).closest('tr').attr('class');
        if (classname == 'invoice_') {
            count++;
        }
    });

    if (count == 0)
        addrows('addinvoiceItem', 'new', '', '', '', '0.00', '0.00', '', '');
    //-------------------------------------------------------------------

    initialload();


    RowTotal();
    cashBalance();
}

//Delete a row of table
function deleteRow(t, item) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'invoice_') alert("Sorry you can't delete this row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    RowTotal();
    var i = 1;
    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
    chkAmount();
    cashBalance();
}
function chkAmount() {
    var tbody = $("#invoicedata tbody");
    if (tbody.children().length == 1) {
        $("#Paying").prop('disabled', false);
    }
}

function RowTotal() {
    var tbody = $("#invoicedata tbody");
    if (tbody.children().length > 0) {
        var totAmt = 0;
        $(".invamt").each(function () {
            var Amt = this.value;
            Amt = Amt || 0;
            totAmt = parseFloat(totAmt) + parseFloat(Amt);
        });
        $("#GrandTotal").val(totAmt.toFixed(2));
        $("#Paying").val(totAmt.toFixed(2));
        //$("#Discount").val(0.00);
    }
}
function setTabIndex() {
    var j = 1;
    $('body').find('input,textarea,select,button, .select2-container .select2-selection__rendered').not(".select2-hidden-accessible").not(":hidden").each(function (i) {
        if (!$(this).hasClass("select2-hidden-accessible") && !$(this).is(":hidden")) {
            $(this).attr('tabindex', j);
            j++;
        }
        if ($(this).closest("tr").hasClass("item_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}

function bindinvoice(data, receiptchk) {
    var chkbox = "";
    if (receiptchk == 0) {
        chkbox = '<th><input type="checkbox" id="select_all" checked="true" /></th>';
    } else {
        chkbox = '<th></th>';
    }

    var table = '<table class="table table-bordered table-hover text-center" id="invoicedata">' +
        '<thead>' +
        '<tr class="bg-gray">' +
        '<th class="text-center">#</th><th class="text-center">Invoice No</th>' +
        '<th class="text-center">Date</th><th class="text-center">Bill Amount</th>' +
        '<th class="text-center">Paid Amount</th><th class="text-center">Balance</th>' + chkbox +
        //'<th class="text-center">Action</th>' +
        '</tr>' +
        '</thead>' +
        '<tbody>';
    var sum = 0;
    var total = 0;
    var paid = 0;
    $.each(data, function (i, item) {
        var balance = item.total - item.paid;
        total += parseFloat(item.total);
        paid += parseFloat(item.paid);
        sum += parseFloat(balance);
        table += '<tr id="row_' + item.bill + '" class="row_' + item.bill + 'billno">';
        table += '<td>' + i + '</td>';
        table += '<td>' + item.invoiceno + '</td>';
        table += '<td>' + convertToDate(item.Date) + '</td>';
        table += '<td class="billamt">' + item.total.toFixed(2) + '</td>';
        table += '<td>' + item.paid.toFixed(2) + '</td>';
        table += '<td class="blnc">' + balance.toFixed(2) + '</td>';
        if (receiptchk == 0) {
            table += '<td><input class="checkbox checkbox-inline" checked="true" type="checkbox" name="bill[]" value="' + item.bill + '" data-bal="' + balance.toFixed(2) + '"/></td>';
        } else {
            table += '<td></td>';
        }
        //       table += '<td><input type="checkbox" name="bill[]" value="' + item.bill + '"/></td>';
        table += '</tr>';
    });
    table += '<tr class="lead"><td colspan="3">Total</td><td>' + total.toFixed(2) + '</td><td>' + paid.toFixed(2) + '</td><td>' + sum.toFixed(2) + '</td><td></td>';
    table += '</tbody></table>';
    $('#invoice').html(table);
    $('#GrandTotal').val(sum.toFixed(2));
}


// find cash balance on entring payment amount
function cashBalance() {
    var totAmt = 0;
    var tbody = $("#invoicedata tbody");
    if (tbody.children().length > 0) {
        $(".invamt").each(function () {
            var Amt = this.value;
            Amt = Amt || 0;
            totAmt = parseFloat(totAmt) + parseFloat(Amt);
        });
    }
    if (totAmt > 0) {

        var totalPayable = $("#GrandTotal").val();
        var totalPaid = totAmt;
        var discount = parseFloat($("#Discount").val());

        //alert(totalPayable + totalPaid + discount);
        totalPayable = totalPayable || 0;
        totalPaid = totalPaid || 0;
        discount = discount || 0;


        var tbody = $("#invoicedata tbody");
        if (tbody.children().length == 1) {
            var payaAmt = totalPaid - discount;
            $("#GrandTotal").val(payaAmt.toFixed(2));
        } else {
            var pay = parseFloat(totalPaid) - parseFloat(discount);
            var balance = parseFloat(totalPaid) - ((parseFloat(totalPaid) - parseFloat(discount)) + parseFloat(discount));
            var GT = parseFloat(totalPaid); //+ parseFloat(discount);
            $("#Paying").val(pay.toFixed(2));
            $("#GrandTotal").val(GT.toFixed(2));
            $("#Balance").val(balance.toFixed(2));


            $("#Discount").val(discount.toFixed(2));
        }
    }
}
// find grand total in case of expense entry
function grandTotal() {
    var subtotal = parseFloat($('#SubTotal').val());
    var taxamount = parseFloat($('#taxamount').val());

    subtotal = subtotal || 0;
    taxamount = taxamount || 0;

    var grandtotal = subtotal + taxamount;
    $('#GrandTotal').val(grandtotal.toFixed(2));
    $('#Paying').val(grandtotal.toFixed(2));
    $('#Balance').val(0);
}
// form submition
function formsubmition(fnval) {
    //var url = $('#recform')[0].action;
    //var data = $('#recform').serialize();

    //var bills = "";
    //var tbody = $("#invoice tbody");
    //if (tbody.children().length > 0) {
    //    tbody.children("tr").each(function () {
    //        var rowid = $(this).attr("id");
    //        if ($("#" + rowid + " .checkbox").prop("checked") == true) {
    //            var bill = $("#" + rowid + " .billno").val();
    //            if (tbody.children().length == 1) {
    //                bills = bill;
    //            } else {
    //                bills = bills + "," + bill;
    //            }
    //        }
    //    });
    //}
    //$("#SelectInvoice").val(bills);

    var url = $('#recform')[0].action;
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.invoice_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;

                });
                data.push(cols);
            });
            return data;
        }
    }
    var invoicedata = HTMLtbl.getData($('#invoicedata'));


    var parameters = {};
    parameters.invoicedata = invoicedata;

    parameters.VoucherNo = $('#VoucherNo').val();
    parameters.Date = $('#Date').val();
    parameters.MOPayment = $('#MoPay').val();
    parameters.PDCDate = $('#PDCDate').val();
    parameters.PayFrom = $('#ddlpayfrom').val();
    parameters.PayTo = $('#ddlpayto').val();
    parameters.CheckNo = $('#CheckNo').val();
    parameters.Bank = $('#Bank').val();
    parameters.GrandTotal = $('#GrandTotal').val();
    parameters.Discount = $('#Discount').val() || 0;
    parameters.Paying = $('#Paying').val();
    parameters.Balance = $('#Balance').val();
    parameters.Remark = $('#Remark').val();
    parameters.Branch = $('#ddlBranch').val();

    parameters.Project = $('#ddlProject').val();
    parameters.ProTask = $('#ddlProTask').val();

    parameters.pdcNote = "";
    parameters.submittype = fnval;
    parameters.debitor = "";
    parameters.creditor = "";

    parameters.Ref1 = $('#Ref1').val();
    parameters.Ref2 = $('#Ref2').val();
    parameters.Ref3 = $('#Ref3').val();
    parameters.Ref4 = $('#Ref4').val();
    parameters.Ref5 = $('#Ref5').val();
    parameters.ApprovedBy = $('#SelApprovedBy').val();
    parameters.ReceiptStatus = $('#ReceiptStatus').val();
    parameters.Override = $('#Override').val();

    $.ajax({
        async: false,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: url,
        data: JSON.stringify(parameters),
        beforeSend: function () {
            $("button").prop('disabled', true); // disable button
        },
        success: function (data) {
            if (data.status == false) {
                $('.ajax_response', res_danger).text(data.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
            else {
                var FileUpload = $("#input-24").get(0);

                var Files = FileUpload.files

                // Create FormData object
                var FileData = new FormData();

                // Looping over all files and add it to FormData object
                for (var i = 0; i < Files.length; i++) {
                    FileData.append(Files[i].name, Files[i]);
                }

                var Mode = $("#Mode").val();

                //To Get the ReceiptID in Edit Mode
                var ReceiptID = $("#ReceiptID").val();

                // Adding one more key to FormData object
                FileData.append("id", ReceiptID);

                //For Uploading Files
                $.ajax({
                    url: '/ReceiptApproval/UploadFiles',
                    type: "POST",
                    contentType: false, // Not to set any content header
                    processData: false, // Not to process data
                    data: FileData,
                    success: function (result) {
                        if (data.status) {
                            if (data.type == 'print') {
                                recprint(data.data, data.tbldata, data.fmapp);
                            }
                            else {
                                $('.ajax_response', res_success).text(data.message);
                                $('.AlertDiv').prepend(res_success);
                            }
                            if (url == null) {

                                window.location.href = '/ReceiptApproval/Create';
                            } else {

                                location.reload();
                            }
                        }
                        else {
                            $('.ajax_response', res_danger).text(data.message);
                            $('.AlertDiv').prepend(res_danger);
                            $("button").prop('disabled', false); // enable button
                        }
                    },
                    error: function (result) {

                    }
                });

                if (data.status) {
                    if (data.type == 'print') {
                        recprint(data.data, data.tbldata, data.fmapp);
                    }
                    else {
                        $('.ajax_response', res_success).text(data.message);
                        $('.AlertDiv').prepend(res_success);
                    }
                    if (url == null) {
                        //  window.location.href = '/ReceiptApproval/Create';
                    } else {
                        // location.reload();
                    }
                }
                else {
                    $('.ajax_response', res_danger).text(data.message);
                    $('.AlertDiv').prepend(res_danger);
                    $("button").prop('disabled', false); // enable button
                }
            }
        }
    })
}











function formsubmitionprop(fnval) {
    //var url = $('#recform')[0].action;
    //var data = $('#recform').serialize();

    //var bills = "";
    //var tbody = $("#invoice tbody");
    //if (tbody.children().length > 0) {
    //    tbody.children("tr").each(function () {
    //        var rowid = $(this).attr("id");
    //        if ($("#" + rowid + " .checkbox").prop("checked") == true) {
    //            var bill = $("#" + rowid + " .billno").val();
    //            if (tbody.children().length == 1) {
    //                bills = bill;
    //            } else {
    //                bills = bills + "," + bill;
    //            }
    //        }
    //    });
    //}
    //$("#SelectInvoice").val(bills);

    var url = $('#recform')[0].action;
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
            table.find('tr').not(':first').not('.invoice_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,textarea,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;

                });
                data.push(cols);
            });
            return data;
        }
    }
    var invoicedata = HTMLtbl.getData($('#invoicedata'));


    var parameters = {};
    parameters.invoicedata = invoicedata;

    parameters.VoucherNo = $('#VoucherNo').val();
    parameters.Date = $('#Date').val();
    parameters.MOPayment = $('#MoPay').val();
    parameters.PDCDate = $('#PDCDate').val();
    parameters.PayFrom = $('#ddlpayfrom').val();
    parameters.PayTo = $('#ddlpayto').val();
    parameters.CheckNo = $('#CheckNo').val();
    parameters.Bank = $('#Bank').val();
    parameters.GrandTotal = $('#GrandTotal').val();
    parameters.Discount = $('#Discount').val() || 0;
    parameters.Paying = $('#Paying').val();
    parameters.Balance = $('#Balance').val();
    parameters.Remark = $('#Remark').val();
    parameters.Branch = $('#ddlBranch').val();

    parameters.Project = $('#ddlProject').val();
    parameters.ProTask = $('#ddlProTask').val();

    parameters.pdcNote = "";
    parameters.submittype = fnval;
    parameters.debitor = "";
    parameters.creditor = "";

    parameters.Ref1 = $('#Ref1').val();
    parameters.Ref2 = $('#Ref2').val();
    parameters.Ref3 = $('#Ref3').val();
    parameters.Ref4 = $('#Ref4').val();
    parameters.Ref5 = $('#Ref5').val();

    $.ajax({
        async: false,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: url,
        data: JSON.stringify(parameters),
        beforeSend: function () {
            $("button").prop('disabled', true); // disable button
        },
        success: function (data) {




            if (data.status) {
                if (data.type == 'print') {
                    recprint(data.data, data.tbldata, data.fmapp);
                }
                else {
                    $('.ajax_response', res_success).text(data.message);
                    $('.AlertDiv').prepend(res_success);
                }
                if (url == null) {
                    window.location.href = '/ReceiptApproval/Create';
                } else {
                    location.reload();
                }
            }
            else {
                $('.ajax_response', res_danger).text(data.message);
                $('.AlertDiv').prepend(res_danger);
                $("button").prop('disabled', false); // enable button
            }
        }
    })
}







function recprint(data, tbldata, fmapp) {

    $("#lblBillNo").text(data.VoucherNo);
    $("#lblDate").text(data.Date);

    $("#lblcreditor").text(data.creditor);
    $("#lblcrediamt").text(data.Paying.toFixed(2));

    $("#lbldebtor").text(data.debitor);
    $("#lblamount").text(data.Paying.toFixed(2));
    $("#lblReference").text(data.User);
    $("#lbltotamt").text(data.Totamt.toFixed(2));
    $("#lblcustomer").text(data.creditor);
    $("#lblmobile").text(data.Phone);
    $("#lblmail").text(data.Email);

    if (data.ComHeadCheck == 0) {
        $("#ComHeadCheck").hide();
    }
    else {
        $("#ComHeadCheck").show();
    }

    if (data.MOPayment == 1 || data.MOPayment == 2)//pdc cdc
    {
        $("#divmop").show();
        $("#divchqno").show();
        $("#lblMOPayment").text(data.MOPay);
        $("#lblChequeNo").text(data.CheckNo);

    } else {
        $("#divmop").hide();
        $("#divchqno").hide();
    }

    if (data.invoicedata != null) {
        var str = "";
        $.each(data.invoicedata, function (i, item) {
            str += "<tr><td>" + item.Type + "</td>" + "<td>" + item.NewRefName + "</td>" + "<td>" + item.Amount.toFixed(2) + "</td>" + "<td></td>" + "<td></td>" + "<td></td></tr>";

        });
        $('#reftable').append(str);
    } else {
        $("#Amttable").hide();
    }

    if (data.Discount == 0) {
        $("#lblcrediamt").text(data.Paying.toFixed(2));
        $("#lbldebitamt").text(data.Paying.toFixed(2));
        $("#lbldebitsum").text(data.Paying.toFixed(2));
        $("#lblcreditsum").text(data.Paying.toFixed(2));
        $('#discount').hide();
        $('#discountid').hide();
    }
    else {
        $("#lblcrediamt").text(data.GrandTotal.toFixed(2));
        $("#lbldebitamt").text(data.Paying.toFixed(2));
        $("#lbldiscount").text(data.Discount.toFixed(2));
        $("#lbldebitsum").text(data.GrandTotal.toFixed(2));
        $("#lblcreditsum").text(data.GrandTotal.toFixed(2));
        $('#discountid').show();
    }

    if (data.MOPayment == 0) {
        $('#checktable').hide();
    }
    else {
        $("#lblcheque").text(data.CheckNo);
        $("#lblpdcdate").text(data.PDCDate);
        if (data.Bank != null) {
            $('#lblbank').text(data.Bank);
        }
        $('#checktable').show();
    }

    if (fmapp != null) {
        $.each(fmapp, function (i, mapp) {

            if (mapp.Field == "Ref1") {
                $("#IblRef1").text(mapp.FieldName);
                $("#IblRef1Val").text(data.Ref1);
                $("#divRef1").show();
            }
            if (mapp.Field == "Ref2") {
                $("#IblRef2").text(mapp.FieldName);
                $("#IblRef2Val").text(data.Ref2);
                $("#divRef2").show();
            }
            if (mapp.Field == "Ref3") {
                $("#IblRef3").text(mapp.FieldName);
                $("#IblRef3Val").text(data.Ref3);
                $("#divRef3").show();
            }
            if (mapp.Field == "Ref4") {
                $("#IblRef4").text(mapp.FieldName);
                $("#IblRef4Val").text(data.Ref4);
                $("#divRef4").show();
            }
            if (mapp.Field == "Ref5") {
                $("#IblRef5").text(mapp.FieldName);
                $("#IblRef5Val").text(data.Ref5);
                $("#divRef5").show();
            }
        });
    }


    $.each(tbldata, function (i, item) {
        var str = '<tr>';
        str += '<td>' + (i + 1) + '</td>';
        str += '<td>' + convertToDate(item.Date) + '</td>';
        str += '<td>' + item.VoucherNo + '</td>';
        str += '<td>' + item.Balance.toFixed(2) + '</td>';
        str += '</tr>';
        $('#itemtable1').append(str);
    });

    var grt = parseFloat(data.GrandTotal).toFixed(2);
    var word = conNumber(grt);
    $('#lblAmtWord').text(word + ' Only');
    if (data.Remark != null) {
        var str1 = '<tr><td><b>Narration</b> : ' + data.Remark + '</td></tr>';
        $('#itemtable2').append(str1);
    }

    if (data.Totamt <= 0) {
        $('#outtable').hide();
    }

    if ($('#hideheader').prop('checked') == true) {
        $('#ComHeadCheck').hide();
        $('#ComfootCheck').hide();
        //$(".invoice.print").css("margin-top", "100px");
        $("#comHeader").css("margin-top", "100px");
    }
    else {
        $('#ComHeadCheck').show();
        $('#ComfootCheck').show();
    }

    var originalpage = document.body.innerHTML;
    var printContent = $('#printit').html();
    $('title').html("Receipt Voucher - " + data.VoucherNo);
    $('body').html(printContent);

    window.print();


}
function sleep(delay) {
    var start = new Date().getTime();
    while (new Date().getTime() < start + delay);
}

function printiteminvoice(e) {
    var total = parseFloat(0);
    $.each(e.item, function (i, item) {
        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;
        var str = '<tr>';
        str += '<td>' + item.Date + '</td>';
        str += '<td>' + item.VoucherNo + '</td>';
        str += '<td>' + item.Balance + '</td>';
        str += '</tr>';
        $('#itemtable1').append(str);
    });
    return true;
}


//Common Methods

function PaymentFrom() {

    // paid to atribute
    $("#ddlpayfrom").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            //url: "/Accounts/SearchAccounts",
            url: "/ReceiptApproval/SearchAccountsEmployee",            
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;

                return {
                    results: data,
                    pagination: {
                        //more: (params.page * 10) < 50
                        more: true
                    }
                };
            },
            cache: true
        },
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });

}
function PaymentTo(EmpId) {
    // paid to atribute
    $("#ddlpayto").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            //url: "/Accounts/SearchPaymentFrom",
            url: "/ReceiptApproval/SearchAccountsPayToEmployee",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    EmployeeId: EmpId
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;
                return {
                    results: data,
                    pagination: {
                        //more: (params.page * 10) < 50
                        more: true
                    }
                };
            },
            cache: true
        },
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });
}

function PayToBankAccount() {

    $("#ddlpayto").select2({
        placeholder: 'Search Account By Code or name',
        minimumInputLength: 0,
        ajax: {
            //url: "/Accounts/SearchPaymentFrom",
            url: "/ReceiptApproval/SearchBankAccounts",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0
                };
            },
            processResults: function (data, params) {
                params.page = params.page || 0;
                return {
                    results: data,
                    pagination: {
                        //more: (params.page * 10) < 50
                        more: true
                    }
                };
            },
            cache: true
        },
        templateResult: SelectToGroup,
        templateSelection: ToSetFormatSelection,
    });   
}

function chkacctype(accid) {
    $.ajax({
        async: true,
        cache: false,
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/Accounts/chkAccountType",
        data: JSON.stringify({ account: accid }),
        success: function (data) {
            if (data == "Supplier") {
                $("#divproject").hide();
            } else if (data == "Customer") {
                $("#divproject").show();

                $('#ddlProject').val(null).trigger('change');
                $('#ddlProTask').val(null).trigger('change');
                CallProjectByAcc();
                CallTask();
            }
            else {
                $("#divproject").show();
                CallProjectAll();
                CallTask();
            }
        }
    });
}