var commcount = 0;

var bcount = 0, btype = '';
blimits = 50;
function paidamountcalculation() {
    var paidAmt = $("#PaidAmount").val();
    var gdTotal = $("#GrandTotal").val();
    if (gdTotal != "" && $("#PaidAmount").is(":visible")) {
        if (parseFloat(gdTotal) < parseFloat(paidAmt)) {
            alert("paid should less than or equals total amount");
            $("#PaidAmount").val("0.00");
            $("#DueAmount").val("0.00");
        }
        else {
            var dues = gdTotal - paidAmt;
            $("#DueAmount").val(parseFloat(dues).toFixed(2));
        }
    }
}
function CalculatetblItemListSum() {
    //alert($("#tot_tax_1").val());

    var tax = $(".tot_tax").val();
    var qty = $(".quty").val();

    //if (tax > 0 || qty != 0) {
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        var gtTax = 0;
        var gtTotal = 0;
        var gtQty = 0;
        var gtSubTotal = 0;
        var gtDiscount = 0;
        var gtRate = 0;

        $(".tot_tax").each(function () {
            var indTax = $(this).val();
            gtTax = parseFloat(gtTax) + parseFloat(indTax);
        });

        $("[id$=total_tax_amount]").text(parseFloat(gtTax).toFixed(2));

        $(".total_price").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });

        $(".totrate").each(function () {
            var ttr = $(this).val();
            ttr = ttr || 0;
            gtRate = parseFloat(gtRate) + parseFloat(ttr);
        });


        $(".quty").each(function () {
            var subQty = this.value;
            gtQty = parseFloat(gtQty) + parseFloat(subQty);
        });

        $(".subtotal").each(function () {
            var subTot = this.value;
            gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
        });

        $(".item_discount").each(function () {
            var subDisc = this.value;
            gtDiscount = parseFloat(gtDiscount) + parseFloat(subDisc);
        });
        gtDiscount = gtDiscount || 0.00;

        // $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
        $("[id$=ToItemPrice]").text((gtRate).toFixed(2));
        $("[id$=total]").text((gtTotal).toFixed(2));
        $("[id$=ToItemCount]").text(tbody.children().length - 1);
        $("[id$=ToItemQnt]").text((gtQty).toFixed(2));
        $("[id$=ToItemAmount]").text((gtSubTotal).toFixed(2));
        $("[id$=ItemDisc]").text(parseFloat(gtDiscount).toFixed(2));
        //$("[id$=TotalAmount]").text((gtSubTotal).toFixed(2));
    }
    //  }
}

function addbillsundryES(t, action, BsValue, AmountType, BsAmount, BsType, BSName, billsundry){
    if (bcount == blimits) alert("You have reached the limit of adding " + bcount + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option = "";
        var readonly = "";
        var row = "<tr class='bs_'>";
        var slno = $('#addbillsundry tr').length + 1;
        tabindex = bcount * 5;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        if (billsundry != null) {
            row = "<tr class='bs_" + billsundry + "'>";
            Option = "<option value='" + billsundry + "'>" + BSName + "</option>";
        }

        if (AmountType == 1) {
            Type = "%";
        } else {
            Type = "";
        }
        if (BsValue == null) {
            BsValue = "";
            readonly = "readonly";
        }


        data = "<td class='text-center'>" + slno + "</td>" +
            "<td class='input-group input-group-sm'><select data-name='BillSundry' name='bsmodel[" + bcount+"].BillSundry' class='form-control bsname' data-id='" + bcount + "' placeholder='Bill Sundry Name' id='bsname'  data-val-required='The bill sundry name field is required' onchange='GetBillSundrydetails(this," + bcount + ")'>" + Option + "</select></td>" +
            "<td><input type='number' data-name='BsValue' name='bsmodel[" + bcount +"].BsValue' " + readonly + " value='" + BsValue + "'  class='form-control bsvalue_" + bcount + "' onchange='bsvaluechange(" + bcount + ");' id='bsvalue_" + bcount + "' data-id='" + bcount + "' id='bsvalue' /></td>" +
            "<td><input type='text' data-name='' value='" + Type + "' class='form-control bsamttype_" + bcount + "' id='bsamttype_" + bcount + "' data-id='" + bcount + "' id='bsamttype' readonly='readonly'/></td>" +
            "<td><input type='number' data-name='BsAmount' name='bsmodel[" + bcount +"].BsAmount' value='" + BsAmount + "' class='form-control bsamt bsamt_" + bcount + "' onchange='bsamtchange(" + bcount + ");' id='bsamt_" + bcount + "' data-id='" + bcount + "' id='bsamt' value='0.00' placeholder='0.00'/><input type='hidden' data-name='AmountType'  value='" + AmountType + "' class='amttypevalue' name='amttypevalue' id='amttypevalue_" + bcount + "'/><input type='hidden' value='" + BsType + "' data-name='BsType'  class='bstype' name='bstype' id='bstype_" + bcount + "'/></td>" +
            "<td class='text-center'><button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deletebsRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>",
            row += data + "</tr>";
        $('#' + t).append(row);
        searchbs();
        bcount++;
      
    }
}
function searchbs() {

    var selecteditem = new Array();
    $(".bsname").each(function () {
        selecteditem.push($(this).val());
    });

    $(".bsname").select2({
        placeholder: 'Search Bill Sundry',
        minimumInputLength: 0,
        ajax: {
            url: "/BillSundry/Search",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    ItemID: selecteditem,
                    page: params.page
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
function GetBillSundrydetails(selectObject, dataid) {
    var SbId = selectObject.value;
    if (SbId != null) {
        if ($(".bs_" + SbId).length > 0) {
            if (confirm('Are you sure want to Add this Bill Sundry Again?')) {
                bsUpdate(selectObject, dataid);
            }
        }
        else {
            bsUpdate(selectObject, dataid);
        }
    }
}
function bsUpdate(selectObject, dataid) {

    $.ajax({
        url: '/BillSundry/GetBillSundryById',
        type: "GET",
        dataType: "JSON",
        data: { bsID: selectObject.value },
        success: function (result) {
            //additive/subtrative
            $("#bstype_" + dataid).val(result.BSType);

            //percentage/amt
            $("#amttypevalue_" + dataid).val(result.AmountType);
            $("#bsvalue_" + dataid).val(result.DefaultValue);
            var defvalue = $("#bsvalue_" + dataid).val();

            BindBsAmount(dataid, defvalue);
            grandtotalcalculation();

            $(selectObject).closest('tr').attr('class', "bs_" + result.BillSundryId);
            if ($(".bs_").length == 0) {
                addbillsundryES('addbillsundry', '', '0.00', '', '0.00', '');
            }

        }
    });
}
//percentage cal on billsundry
function calculatePercentage(dataid) {
    var total = parseFloat($("#total").text());
    total = (total > 0) ? total : 0;
    var value = parseFloat($("#bsvalue_" + dataid).val());
    var amt = (total * (value / 100));
    $("#bsamt_" + dataid).val(amt.toFixed(2));
}

function BindBsAmount(dataid, defvalue) {
    var value = parseFloat($("#bsvalue_" + dataid).val());
    var bstype = parseFloat($("#bstype_" + dataid).val());
    var amtype = $("#amttypevalue_" + dataid).val();
    var total = parseFloat($("#total").text());


    total = (total > 0) ? total : 0;
    if (amtype == 0) {
        $("#bsvalue_" + dataid).val("").attr('readonly', true);
        $("#bsamttype_" + dataid).val("");
        $("#bsamt_" + dataid).val(parseFloat(defvalue).toFixed(2));
        $("#bsamt_" + dataid).focus();
    } else {
        $("#bsvalue_" + dataid).focus();
        $("#bsvalue_" + dataid).val(defvalue);
        $("#bsamttype_" + dataid).val("%");
        $("#bsvalue_" + dataid).attr('readonly', false);
        calculatePercentage(dataid);
    }
    grandtotalcalculation();
}
function grandtotalcalculation() {
    var gtTotal = parseFloat($("#total").val());
    gtTotal = (gtTotal > 0) ? gtTotal : 0;

    $("#addbillsundry tr").each(function () {
        var type = parseFloat($(this).find('.bstype').val());
        var amt = $(this).find('.bsamt').val();

        amt = (amt > 0) ? amt : 0;
        if (type == 0) {
            gtTotal = parseFloat(gtTotal) + parseFloat(amt);
        } else if (type == 1) {

            gtTotal = parseFloat(gtTotal) - parseFloat(amt);
        }
    });
    $("#GrandTotal").val(parseFloat(gtTotal).toFixed(2));
    paidamountcalculation();
}

//onchange of billsundry value
function bsvaluechange(arg) {
    var defvalue = $("#bsvalue_" + arg).val();
    BindBsAmount(arg, defvalue);
}
//amt chnage
function bsamtchange(arg) {
    var defvalue = parseFloat($("#bsamt_" + arg).val());
    BindBsAmount(arg, defvalue);
}

//Delete a row of table
function deletebsRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'bs_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    grandtotalcalculation();
    var i = 1;
    $('#addbillsundry tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

//print item bill sundry
//itembind
function bindItem(e, type) {
    var total = parseFloat(0);
    var str = "";
    var count = 1;
    $.each(e.item, function (i, item) {


        var subtot = parseFloat(item.ItemTotalAmount.toFixed(2));
        total += subtot;
        var itemnote = "";
        if (item.ItemNote != "") {
            itemnote = "<br /><small>" + item.ItemNote + "</small>";
        }
        var unit = (item.ItemUnit != null) ? item.ItemUnit : "";
        str += '<tr>';
        str += '<td>' + count + '</td>';
        str += '<td>' + item.ItemName + " " + unit + itemnote + '</td>';
        // str += '<td>' + unit + '</td>';
        str += '<td>' + item.ItemQuantity + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemUnitPrice).toFixed(2) + '</td>';
        str += '<td class="text-right">' + parseFloat(item.ItemSubTotal).toFixed(2) + '</td>';
        if (type == "sales") {
            str += '<td style="text-align:right;">' + (item.ItemTax) + "%" + '</td>';
        }
        str += '<td class="text-right">' + parseFloat(item.ItemTaxAmount).toFixed(2) + '</td>';
        str += '<td class="text-right">' + subtot.toFixed(2) + '</td>';
        str += '</tr>';
        count++;
    });
    return str;
}
// bind bill sundry
function bindSundry(e) {
    var str = "";
    $.each(e.billsundry, function (i, billsundry) {
        var type = "";
        var type2 = "";
        var symbol = "";
        var value = "";
        if (billsundry.BsType == 0) {
            type = "Add";
        } else {
            type = "Less";
        }
        if (billsundry.AmountType == 1) {
            type2 = "&#64;";
            symbol = "%";
        } else {
            type2 = "";
            symbol = "";
        }
        if (billsundry.BsValue > 0) {
            value = parseFloat(billsundry.BsValue).toFixed(2);
        } else {
            value = "";
        }
        if (billsundry.BsAmount > 0) {
            str += '<tr class="border-top">';
            str += '<td></td>';
            str += '<td></td>';
            str += '<td>' + billsundry.BillSundry + '</td>';
            str += '<td >' + parseFloat(billsundry.BsAmount).toFixed(2) + '</td>';
            str += '</tr>';
        }
    });
    return str;
}

function addrowcomm(commid, agent, commisiontype, commisionmode, comvalue, agentvalue) {




    agent = (agent == undefined) ? "" : agent;
    agentvalue = (agentvalue == undefined) ? "" : agentvalue;
    commid = (commid == undefined) ? 0 : commid;
    commisiontype = (commisiontype == undefined) ? "" : commisiontype;
    commisionmode = (commisionmode == undefined) ? "" : commisionmode;
    comvalue = (comvalue == undefined) ? "" : comvalue;
    //ED = (ED == undefined) ? "" : convertToDate(ED);
    //Note = (Note == undefined) ? "" : Note;
    if (agentvalue != null) {
        Option = "<option value='" + agent + "'>" + agentvalue + "</option>";
    }
    if (commisiontype == "") {
        commisiontypeOption = '<option value="1"  selected>Percentage</option>' +
            '<option value="2">Lum sum</option>';
    }

    else {
        if (commisiontype == 1) {
            commisiontypeOption = '<option value="1" selected>Percentage</option>' +
                '<option value="2">Lum sum</option>';
        }

        else {
            commisiontypeOption = '<option value="1" >Percentage</option>' +
                '<option value="2" selected>Lum sum</option>';
        }
    }
    if (commisionmode == "") {
        commisionmodeOption = '<option value="1" selected>Net Profit</option>' +
            '<option value="2">Gross Profit</option>';
    }

    else {
        if (commisionmode == 1) {
            commisionmodeOption = '<option value="1" selected>Net Profit</option>' +
                '<option value="2">Gross Profit</option>';
        }

        else {
            commisionmodeOption = '<option value="1">Net Profit</option>' +
                '<option value="2" selected>Gross Profit</option>';
        }
    }

    var htmlcomm = '<tr class="commrow"><td class="text-center" style="width:30%">' +
        '<select class="form-control agent" onclick="salesExecPopUpcom();"  id="agent' + commcount + '" name="commssion[' + commcount + '].agent"  data-name="agent" placeholder="Select Agent" >' + Option + '</select>' +
        ' </td>' +
        '<td class="text-center">' +
        '<select class="form-control commisiontype" value="' + commisiontype + '" id="commisiontype' + commcount + '" name="commssion[' + commcount + '].commisiontype" data-name="commisiontype"  type="text" placeholder="Select commision type">' +
        commisiontypeOption +
        '</select>' +

        '</td>' +
        '<td class="text-center">' +
        '<select class="form-control commisionmode" data-name="commisionmode" value="' + commisionmode + '" id="commisionmode' + commcount + '" name="commssion[' + commcount + '].commisionmode" type="text" placeholder="Select commision mode">' +
        commisionmodeOption +
        '</select>' +
        '</td>' +


        '<td class="text-center">' +

        '<input class="form-control comvalue" data-name="comvalue" value="' + comvalue + '" id="comvalue' + commcount + '" name="commssion[' + commcount + '].comvalue" type="text" placeholder="Enter Value" />' +

        '</td>' +
        '<td class="text-center"> <button type="button" class="btn btn-flat btn-success" onclick="addrowcomm()">Add <i class="fa fa-plus"></i></button>&nbsp;<button type="button" class="btn btn-flat btn-danger" onclick="deleteRowDoc(this,' + commcount + ',' + commid + ')">Delete <i class="fa fa-trash"></i></button></span ></td>' +

        '</tr>'

    
    $(htmlcomm).appendTo($("#normalcommission"));
    commcount++;
    //  resetDocbtn();


    salesExecPopUpcom();
}

function Amtchange(object, arg) {
    var rent = parseFloat($('#Rent').val());
    var amount = parseFloat($('#Amount').val());
    var ContAmount = parseFloat($('#ContractAmount').val());
    var gtTotal = 0;
    if (rent > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > rent) {
            //  $('#dname_' + arg).val(0);
            //  alert("Cheque amount excceeds Rent..")
        }
    }
    if (ContAmount > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > ContAmount) {
            //  $('#dname_' + arg).val(0);
            //  alert("Cheque amount excceeds Contract Amount..")
        }
    }
    if (amount > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > amount) {
            // $('#dname_' + arg).val(0);
            // alert("Cheque amount excceeds Contract Amount..")
        }
    }
}

function resetDocbtn() {
    var i = 0;
    var mbLen = $(".commrow").length;

    $('.commrow').each(function (index, element) {
        var input1 = $(this).find('.DName');
        input1.attr('name', 'cheqmodel[' + i + '].Amount');

        var input2 = $(this).find('.DNo');
        input2.attr('name', 'cheqmodel[' + i + '].ChequeNo');

        var input3 = $(this).find('.ISdate');
        input3.attr('name', 'cheqmodel[' + i + '].Date');

        var input6 = $(this).find('.Attach');
        input6.attr('name', 'cheqmodel[' + i + '].Attachments');

        var input7 = $(this).find('.BankName');
        input7.attr('name', 'cheqmodel[' + i + '].Bank');

        var dltbtn = $(this).find('.ed-dlt');
        var addbtn = $(this).find('.ed-add');
        if (index === (mbLen - 1)) {
            if (addbtn.hasClass('hide')) {
                addbtn.removeClass('hide');
            }
            if (!dltbtn.hasClass('hide')) {
                dltbtn.addClass('hide');
            }
        }
        else {
            if (!addbtn.hasClass('hide')) {
                addbtn.addClass('hide');
            }
            if (dltbtn.hasClass('hide')) {
                dltbtn.removeClass('hide');
            }
        }
        i++;
    });

}
function deleteRowDoc(t, arg, id) {

    if (commcount == 1) alert("Sorry You Can't Delete This Row.");
    else {

        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        commcount--;

    }
    // resetDocbtn();
}
function salesExecPopUpcom() {

    $(".agent").select2({
        placeholder: 'Search Sales Person by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployee",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page
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



function searchBankName() {

    $(".BankName").select2({
        placeholder: 'Search Bank',
        minimumInputLength: 0,
        ajax: {
            url: "/Master/SearchBankAccounts",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all",
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

function typechange(selectedObject, dataid) {
    searchBankName();
}
function Addcheqr() {
    AddCheq();
    searchBankName();
}
function commissionsPopUp() {
    $.fn.modal.Constructor.prototype.enforceFocus = function () { };
    $('#modal-commission').on('shown.bs.modal', function (e) {
        var tbody1 = $("#normalcommission tbody");
        if (tbody1.children().length == 0) {
            addrowcomm();
            salesExecPopUpcom();
        }
    });
    $("#modal-commission").modal({ show: true, backdrop: "static" });
}




function addrowesti(estimateid, date, invoiceno, description, amount) {



    estimateid = (estimateid == undefined) ? 0 : estimateid;
    date = (date == undefined) ? "" : date;

    invoiceno = (invoiceno == undefined) ? "" : invoiceno;
    description = (description == undefined) ? "" : description;
    amount = (amount == undefined) ? "" : amount;
    //ED = (ED == undefined) ? "" : convertToDate(ED);
    //Note = (Note == undefined) ? "" : Note;
   //EstimateItemId
    var slno = commcount + 1;
    var htmlcomm = '<tr class="estrow"><input type="hidden" name="QuotItem[' + commcount +'].EstimateItemId" value="0"/><td class="text-center">' + slno +
        '</td><td><div class="input-group date">'+
        '<div class="input-group-addon" > <i class="fa fa-calendar"></i></div ><input class="form-control estdate datepicker" data-name="estdate" value="' + date + '" id="estdate' + commcount + '" name="QuotItem[' + commcount + '].invdate" type="text" placeholder="Enter Value" /></div>' +
        '</td><td>' +
            '<input class="form-control estinvno" data-name="estinvno" value="' + invoiceno + '" id="estinvno' + commcount + '" name="QuotItem[' + commcount + '].invno" type="text" placeholder="Enter Invoice NO" />' +
        '</td><td>' +
            '<input class="form-control estdescription" data-name="estdescription" value="' + description + '" id="estdescription' + commcount + '" name="QuotItem[' + commcount + '].description" type="text" placeholder="Enter Description" />' +
        '</td><td>' +
        '<input class="form-control estamount" data-name="estamount" value="' + amount + '" id="estamount' + commcount + '" name="QuotItem[' + commcount + '].amount" type="number" placeholder="Enter Amount" onkeyup="caltotal();" style="text-align: right;" />' +
        '</td>' +     
        '<td class="text-center"> <button type="button" class="btn btn-flat btn-success" onclick="addrowesti()">Add <i class="fa fa-plus"></i></button>&nbsp;<button type="button" class="btn btn-flat btn-danger" onclick="deleteRowDoc(this,' + commcount +')">Delete <i class="fa fa-trash"></i></button></span ></td>' +
        '</tr>'
   
 
    $(htmlcomm).appendTo($("#addinvoiceItem"));
    commcount++;
    adddate();
    //  resetDocbtn();
 caltotal();


}
function caltotal() {
    var i;
  var total = 0;
    for (i = 0; i < commcount; i++) {
        if ($("#estamount" + i).val()!="")
        total = total + parseFloat($("#estamount" + i).val());

    }
    $("#total").val(total);
    grandtotalcalculation();
}

function adddate() {
    $('.date').datepicker({
         format: 'dd-mm-yyyy',
        autoclose: true,
        allowInputToggle: true,
        startDate: '01-01-2011',
        endDate: '+30d'
    });
    jQuery.validator.methods["date"] = function (value, element) { return true; }
}

function Amtchange(object, arg) {
    var rent = parseFloat($('#Rent').val());
    var amount = parseFloat($('#Amount').val());
    var ContAmount = parseFloat($('#ContractAmount').val());
    var gtTotal = 0;
    if (rent > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > rent) {
            //  $('#dname_' + arg).val(0);
            //  alert("Cheque amount excceeds Rent..")
        }
    }
    if (ContAmount > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > ContAmount) {
            //  $('#dname_' + arg).val(0);
            //  alert("Cheque amount excceeds Contract Amount..")
        }
    }
    if (amount > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > amount) {
            // $('#dname_' + arg).val(0);
            // alert("Cheque amount excceeds Contract Amount..")
        }
    }
}

function resetDocbtn() {
    var i = 0;
    var mbLen = $(".commrow").length;

    $('.commrow').each(function (index, element) {
        var input1 = $(this).find('.DName');
        input1.attr('name', 'cheqmodel[' + i + '].Amount');

        var input2 = $(this).find('.DNo');
        input2.attr('name', 'cheqmodel[' + i + '].ChequeNo');

        var input3 = $(this).find('.ISdate');
        input3.attr('name', 'cheqmodel[' + i + '].Date');

        var input6 = $(this).find('.Attach');
        input6.attr('name', 'cheqmodel[' + i + '].Attachments');

        var input7 = $(this).find('.BankName');
        input7.attr('name', 'cheqmodel[' + i + '].Bank');

        var dltbtn = $(this).find('.ed-dlt');
        var addbtn = $(this).find('.ed-add');
        if (index === (mbLen - 1)) {
            if (addbtn.hasClass('hide')) {
                addbtn.removeClass('hide');
            }
            if (!dltbtn.hasClass('hide')) {
                dltbtn.addClass('hide');
            }
        }
        else {
            if (!addbtn.hasClass('hide')) {
                addbtn.addClass('hide');
            }
            if (dltbtn.hasClass('hide')) {
                dltbtn.removeClass('hide');
            }
        }
        i++;
    });

}
function deleteRowDoc(t, arg, id) {
    
    if (commcount == 1) alert("Sorry You Can't Delete This Row.");
    else {
       
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
            commcount--;
       
    }
   // resetDocbtn();
}


function deleteRowDocest(t, arg) {

    if (commcount == 1) alert("Sorry You Can't Delete This Row.");
    else {

        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        commcount--;

    }
    // resetDocbtn();
}

function searchBankName() {

    $(".BankName").select2({
        placeholder: 'Search Bank',
        minimumInputLength: 0,
        ajax: {
            url: "/Master/SearchBankAccounts",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all",
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

function typechange(selectedObject, dataid) {
    searchBankName();
}
function Addcheqr() {
    AddCheq();
    searchBankName();
}
function commissionsPopUp() {
    $.fn.modal.Constructor.prototype.enforceFocus = function () { };
    $('#modal-commission').on('shown.bs.modal', function (e) {
        var tbody1 = $("#normalcommission tbody");
        if (tbody1.children().length == 0) {
            addrowcomm();
            salesExecPopUpcom();
        }
    });
    $("#modal-commission").modal({ show: true, backdrop: "static" });
}

