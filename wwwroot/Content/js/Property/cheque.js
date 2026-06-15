var checount = 0;
function AddCheq(ID, Amt, No, Date, AttAch, Id, Bank, BankName) {

    var previouscount = parseFloat(checount) - 1;
    previouscount = (previouscount == undefined) ? "" : previouscount;
    var previous = $('#doc_' + previouscount).val();


    var req = "";
    var payroll = $("#chkpayroll").val();
    if (checount >= 1) {
        req = 'required = "required"';
    }
    if (1 == 1) {
        var docmod = $("input[name='cheqmodel[" + previouscount + "].Amount']").val();

        Amt = (Amt == undefined) ? "" : Amt;
        ID = (ID == undefined) ? 0 : ID;
        No = (No == undefined) ? "" : No;
        Date = (Date == undefined) ? "" : convertToDate(Date);
        //ED = (ED == undefined) ? "" : convertToDate(ED);
        //Note = (Note == undefined) ? "" : Note;

        AttAch = (AttAch == undefined) ? "" : AttAch;       //alert(Id)
        var attachimage = "";
        var empid = $("#id").val();
        if (AttAch != "") {
            // attachimage = '<img class="img-responsive editclogo" alt=' + AttAch + ' src="/uploads/chequeimage/' + AttAch + '" />'
            attachimage = '<a  alt=' + AttAch + ' href="/uploads/chequeimage/' + AttAch + '"  target="_new">' + AttAch + '</a>'
        }
        var Option = "";
        BankName = (BankName == undefined) ? "" : BankName;
        if (BankName != null) {
            Option = "<option value='" + Bank + "'>" + BankName + "</option>";
        }
        if (1 == 1) {
            var html = '<div class="docSet">' +
                '<div class="row">' +
                '<div class="form-group"><input  value="' + ID + '" id="checkid' + checount + '" name="cheqmodel[' + checount + '].ID"  value="' + ID + '" type="hidden" /></div>' +
                '<div class="form-group col-md-2"><label class="control-label">Amount</label><input class="form-control DName" onchange="Amtchange(this,' + checount + ')" value="' + Amt + '" id="dname_' + checount + '" name="cheqmodel[' + checount + '].Amount" type="text" placeholder="Enter Document Name"/></div>' +
                '<div class="form-group col-md-2"><label class="control-label">Cheque No & Bank</label><input class="form-control DNo" value="' + No + '" id="dnum_' + checount + '" name="cheqmodel[' + checount + '].ChequeNo" type="text" placeholder="Enter Document No" /></div>' +
                '<div class="form-group col-md-2"><label class="control-label">Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker issd ISdate" value="' + Date + '" id="issdate_' + checount + '" name="cheqmodel[' + checount + '].Date" type="text" placeholder="Enter Date" /></div></div>' +
                //'<div class="form-group col-md-2" style="display:none;"><label class="control-label">Bank</label><select class="form-control BankName" onclick="searchBankName();" value="' + BankName + '" id="bank_' + checount + '" name="cheqmodel[' + checount + '].Bank" type="text" placeholder="Enter Bank Name">' + Option + '</select></div>' +
                '<div class="form-group col-md-2"><label class="control-label">Attachments</label><input type="file" class="form-control Attach" value="' + AttAch + '" id="dattach_' + checount + '" name="cheqmodel[' + checount + '].Attachments" type="text" placeholder="Enter Attachments" />' + attachimage + '</div>' +
                '<div class="form-group col-md-1 text-right"><label class="control-label"></label><span class="input-group-btn ed-add"><button type="button" class="btn btn-flat btn-success"  onclick="AddCheq()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn ed-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowDoc(this,' + checount + ',' + ID + ')">Delete <i class="fa fa-trash"></i></button></span></div>' +

                '</div>'
            $(html).appendTo($("#CheqDetails"));
            checount++;
            resetDocbtn();


            $(".issd").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });

            jQuery.validator.methods["date"] = function (value, element) { return true; }
        }
    }
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
    var mbLen = $(".docSet .row").length;

    $('.docSet .row').each(function (index, element) {
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
    var classname = $(t).closest('div').attr('class');
    if (checount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        if (id > 0) {


            var url = "/Property/PropertyRegistration/Deletecheque/" + id;
            var data = $('#deleteform').serialize();
            //  createajax(url, data, '#modal-delete');
            //modalshow("/Property/PropertyRegistration/Deletecheque/" + id, '#modal-delete');
            $.ajax({
                type: "POST",
                url: url,
                data: data,
                success: function (data) {
                    if (data.status) {


                        var e = t.parentNode.parentNode.parentNode;
                        e.parentNode.removeChild(e);
                        checount--;
                        if (typeof oTable != 'undefined')
                            oTable.draw(false);
                    }
                    else {
                        ////for (var i = 0; i < data.errors.length; i++) { 
                        //    $('.ajax_response', res_danger).text(data.error[0]);
                        //    $('.AlertDiv').prepend(res_danger);
                        ////}

                        $('.ajax_response', res_danger).text(data.message);
                        $('.AlertDiv').prepend(res_danger);
                    }
                    fadeAlert();
                }
            })
        }
        if (id == 0) {
            var e = t.parentNode.parentNode.parentNode;
            e.parentNode.removeChild(e);
            checount--;
        }
    }
    resetDocbtn();
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
