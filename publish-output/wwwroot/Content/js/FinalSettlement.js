var FinalcountAdd = 1, type = '';
limits = 500;

var Payded = [];
var dltcount = 0;
var dltdedcount = 0;
function AddFsadd(t, action, add, addamt, name) {   
    if (FinalcountAdd == limits) alert("You have reached the limit of adding " + FinalcountAdd + " inputs");
    else {
        var Option = "";
        var required = "";
        var divid = "fs_" + add;
        var data = "";
        var a = "item_name" + FinalcountAdd,
        tabindex = FinalcountAdd * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        //action = (action == 'create') ? "" : action;
        if (action == 'edit') {
            Option = "<option value='" + add + "'>" + name + "</option>";
        }
        var slno = $('#fsaddbody tr').length + 1;
        var row = "<tr class='fs' id='fs_" + FinalcountAdd + "'>";
        data =                                           
                "<td class='form-group col-md-2'><select class='form-group Addit' value='" + add + "' id='add_" + FinalcountAdd + "' name='Additiondetails[" + (FinalcountAdd) + "].Payhead' type='text' placeholder='Select Addition Name'>" + Option + "</select></td> " +
                "<td class='form-group col-md-2'><input  class='addamt_" + FinalcountAdd + " form-control text-left Addamt' name='Additiondetails[" + (FinalcountAdd ) + "].Amount' tabindex='" + tab2 + "' data-msg-required='The Amount field is required' value='" + addamt + "' id='addamt_" + FinalcountAdd + "' onchange='calculatetotal()' placeholder='Enter addition amount' min='0''/></td> " +
                "<td><button type='button' class='btn btn-flat btn-success fs-add' onclick='addRow(this," + FinalcountAdd + ")'><i class='fa fa-plus'></i></button>" +
                "<button type='button' class='btn btn-flat btn-danger fs-dlt hide' onclick='deletefsRow(this," + FinalcountAdd + ")'><i class='fa fa-1x fa-plus-circle'></i></tr>";
        
        row += data + "</tr>";
        $('#' + t).append(row);
        FinalcountAdd++;
        resetearnbtn();
    }
}


var Finalcount = 1, type = '';
function AddFsded(t, action, ded, dedamt,name) {
    if (Finalcount == limits) alert("You have reached the limit of adding " + Finalcount + " inputs");
    else {
        var Option = "";
        var required = "";
        var divid = "fs_" + ded;
        var data = "";
        var a = "deditem_name" + Finalcount,
        tabindex = Finalcount * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        //action = (action == 'create') ? "" : action;
        if (action == 'edit') {
            Option = "<option value='" + ded + "'>" + name + "</option>";
        }
        var slno = $('#fsdedbody tr').length + 1;
        var row = "<tr class='dedfs' id='dedfs_" + Finalcount + "'>";
        data =
                "<td class='form-group col-md-2'><select class='form-group col-md-2 Deduct' value='" + ded + "' id='ded_" + Finalcount + "' name='Deductiondetails[" + (Finalcount) + "].Payhead' type='text' placeholder='Select Deduction Name'>" + Option + "</select></td> " +
                "<td class='form-group col-md-2'><input  class='dedamt_" + FinalcountAdd + " form-control text-left dedamt' name='Deductiondetails[" + (Finalcount) + "].Amount' tabindex='" + tab2 + "' data-msg-required='The to field is required' value='" + dedamt + "' id='dedamt_" + Finalcount + "' onchange='deddata()' class='dedamt_" + Finalcount + " form-control text-left to' placeholder='Enter deduction amount' min='0''/></td> " +
                "<td><button type='button' class='btn btn-flat btn-success dedfs-add'  onclick='addRowDed(this," + Finalcount + ")'><i class='fa fa-plus'></i></button>" +
                "<button type='button' class='btn btn-flat btn-danger dedfs-dlt hide'  onclick='deletededRow(this," + Finalcount + ")'><i class='fa fa-1x fa-plus-circle'></i></td>";
        "</td>";
        row += data + "</tr>";
        $('#' + t).append(row);
        Finalcount++;
        resetdedbtn();
    }
}

//Earnings
function searchFinalAddType() {
    var selecteditem = new Array();
    var section = $('#Section').val();
    var cot = 1;
    var addarray = [];
    $(".Addit").each(function () {
        selecteditem.push($(this).val());
        var addeditem = $('#add_'+cot).val();
        if (addeditem != undefined) {  
            addarray.push(addeditem)
            cot++;
        }
    });

    $(".Addit").select2({
        placeholder: 'Search Addition Payhead',
        minimumInputLength: 0,
        ajax: {
            url: "/Hr/Payhead/searchFinalAddType",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all",
                    Add: addarray,
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
function addRow(object, arg) {
    var data = $('#add_' + arg).val(); 
    var tblcount = parseInt(dltcount) + ($('#fsaddbody tr').length);
    if (tblcount==arg && (data != "" && data != null)) {
        AddFsadd('fsaddbody', 'contact', "", "");
        searchFinalAddType();
    }
}
function deletefsRow(t, arg) {
    var classname = $(t).closest('tr').attr('id'); 
    if (($('#fsaddbody tr').length) == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        FinalcountAdd--;
        dltcount++;
    } 
    resetearnbtn();
    calculatetotal();
}
function calculatetotal() {
    var netamt = ($('#GratuityAmount').val() == "") ? 0 : $('#GratuityAmount').val();
    if (netamt == undefined) {
        netamt = ($('#LeaveSalary').val() == "") ? 0 : $('#LeaveSalary').val();
    }
    var total = netamt;
    //earnings
    var tbody = $("#fsaddtable tbody");
    if (tbody.children().length > 0) {
        tbody.children("tr").each(function () {
            var rowid = $(this).attr("id"); 
            var amt = $("#" + rowid + " .Addamt").val(); 
            amt = (amt == "")  ? 0 : amt;    
            total = parseFloat(amt) + parseFloat(total);
        });
    }
    //deductions
    var tbodyded = $("#fsdedtable tbody");
    if (tbodyded.children().length > 0) {
        tbodyded.children("tr").each(function () {
            var rowid = $(this).attr("id");
            var amt = $("#" + rowid + " .dedamt").val();
            amt = (amt == "") ? 0 : amt;
            total = parseFloat(total) - parseFloat(amt);
        });
    }
    $('#NetAmount').val(total.toFixed(2));
}
function resetearnbtn() {
    var i = 0;
    var EarLen = $(".fs").length;
    $('.fs').each(function (index, element) {
        var inputPay = $(this).find('.Addit');
        var inputAmt = $(this).find('.Addamt');
        inputPay.attr('name', 'Additiondetails[' + i + '].Payhead');
        inputAmt.attr('name', 'Additiondetails[' + i + '].Amount');
        var dltbtn = $(this).find('.fs-dlt');
        var addbtn = $(this).find('.fs-add');
        if (index === (EarLen - 1)) {
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

//deduction
function searchFinaldedType() {
    var selecteditem = new Array();
    var section = $('#Section').val();
    var cot = 1;
    var addarray = [];
    $(".Deduct").each(function () {
        selecteditem.push($(this).val());
        var addeditem = $('#ded_' + cot).val();
        if (addeditem != undefined) {
            addarray.push(addeditem)
            cot++;
        }
    });
    $(".Deduct").select2({
        placeholder: 'Search Deduction Payheade',
        minimumInputLength: 0,
        ajax: {
            url: "/Hr/Payhead/searchFinaldedType",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all",
                    Ded: addarray,
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


function addRowDed(object, arg) {
    var data = $('#ded_' + arg).val();
    var tblcount = parseInt(dltdedcount) + ($('#fsdedbody tr').length); 
    if ((tblcount == arg) && (data != "" && data != null)) {
        AddFsded('fsdedbody', 'contact', "", "");
        searchFinaldedType();
    }
}
function deletededRow(t, arg) {
    var classname = $(t).closest('tr').attr('id');
    if (($('#fsdedbody tr').length) == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        Finalcount--;
        dltdedcount++;
    }
    resetdedbtn();
    calculatetotal();
}
function deddata() {
    calculatetotal();

}
function resetdedbtn() {
    var i = 0;
    var EarLen = $(".dedfs").length;
    $('.dedfs').each(function (index, element) {
        var inputMb = $(this).find('.Deduct');
        var inputAmt = $(this).find('.dedamt');
        inputMb.attr('name', 'Deductiondetails[' + i + '].Payhead');
        inputAmt.attr('name', 'Deductiondetails[' + i + '].Amount');

        //inputMb.attr('id', 'add_' + i);
        //inputMb.attr('id', 'addamt_' + i);
        var dltbtn = $(this).find('.dedfs-dlt');
        var addbtn = $(this).find('.dedfs-add');
        if (index === (EarLen - 1)) {
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
