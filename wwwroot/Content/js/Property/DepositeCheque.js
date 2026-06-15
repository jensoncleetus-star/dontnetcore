var Dchecount = 0;
function AddDepCheq(ID,Amt, No, Date, AttAch, Id, Bank, BankName) {

    var previouscount = parseFloat(Dchecount) - 1;
    previouscount = (previouscount == undefined) ? "" : previouscount;
    var previous = $('#doc_' + previouscount).val();
    var req = "";
    var payroll = $("#chkpayroll").val();
    if (Dchecount >= 1) {
        req = 'required = "required"';
    }
    //Dchecount == 0 || previous != ""
    if (1==1) {
        var docmod = $("input[name='cheqmodeldep[" + previouscount + "].Amount']").val();

        Amt = (Amt == undefined) ? "" : Amt;
        No = (No == undefined) ? "" : No;
        Date = (Date == undefined) ? "" : convertToDate(Date);
        ID = (ID == undefined) ? 0 : ID;
        //ED = (ED == undefined) ? "" : convertToDate(ED);
        //Note = (Note == undefined) ? "" : Note;

        AttAch = (AttAch == undefined) ? "" : AttAch;       //alert(Id)
        var attachimage = "";
        var empid = $("#id").val();
        if (AttAch != "") {
           
            attachimage = '<a  alt=' + AttAch + ' href="/uploads/chequeimage/' + AttAch + '"  target="_new">' + AttAch + '</a>'
        }
        var Option = "";
        BankName = (BankName == undefined) ? "" : BankName;
        if (BankName != null) {
            Option = "<option value='" + Bank + "'>" + BankName + "</option>";
        }
        if (Dchecount == 0 || docmod != "") {
            var html = '<div class="depSet">' +
                '<div class="row">' +
                '<div class="form-group"><input  value="' + ID + '" id="cheqmodeldep' + checount + '" name="cheqmodeldep[' + Dchecount + '].ID"  value="' + ID + '" type="hidden" /></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Amount</label><input class="form-control DepName" onchange="Amtchange(this,' + Dchecount + ')" value="' + Amt + '" id="depname_' + Dchecount + '" name="cheqmodeldep[' + Dchecount + '].Amount" type="text" placeholder="Enter Document Name"/></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Cheque No & Bank</label><input class="form-control DNo" value="' + No + '" id="dnum_' + Dchecount + '" name="cheqmodeldep[' + Dchecount + '].ChequeNo" type="text" placeholder="Enter Document No" /></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker issd ISdate" value="' + Date + '" id="issdate_' + Dchecount + '" name="cheqmodeldep[' + Dchecount + '].Date" type="text" placeholder="Enter Date" /></div></div>' +
           // '<div class="form-group col-md-2" style="display:none;"><label class="control-label">Bank</label><select class="form-control BankName" onclick="searchBankName();" value="' + BankName + '" id="bank_' + Dchecount + '" name="cheqmodeldep[' + Dchecount + '].Bank" type="text" placeholder="Enter Bank Name">' + Option + '</select></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Attachments</label><input type="file" class="form-control Attach" value="' + AttAch + '" id="dattach_' + Dchecount + '" name="cheqmodeldep[' + Dchecount + '].Attachments" type="text" placeholder="Enter Attachments" />' + attachimage + '</div>' +
                '<div class="form-group col-md-1 text-right"><label class="control-label"></label><span class="input-group-btn Dep-ed-add"><button type="button" class="btn btn-flat btn-success"  onclick="AddDepCheq()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn Dep-ed-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowDep(this,' + Dchecount + ',' + ID +')">>Delete <i class="fa fa-trash"></i></button></span></div>' +

            '</div>'
            $(html).appendTo($("#DepCheqDetails"));
            Dchecount++;
            resetDocbtnDep();


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
        $(".DepName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > rent) {
           /// $('#depname_' + arg).val(0);
           // alert("Cheque amount excceeds Rent..")
        }
    }
    if (ContAmount > 0) {
        $(".DepName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > ContAmount) {
           // $('#depname_' + arg).val(0);
          //  alert("Cheque amount excceeds Contract Amount..")
        }
    }
    if (amount > 0) {
        $(".DName").each(function () {
            var indtot = $(this).val();
            gtTotal = parseFloat(gtTotal) + parseFloat(indtot);
        });
        if (gtTotal > amount) {
          //  $('#dname_' + arg).val(0);
           // alert("Cheque amount excceeds Contract Amount..")
        }
    }
}

function resetDocbtnDep() {
    var i = 0;
    var mbLen = $(".depSet .row").length;

    $('.depSet .row').each(function (index, element) {
        var input1 = $(this).find('.DepName');
        input1.attr('name', 'cheqmodeldep[' + i + '].Amount');

        var input2 = $(this).find('.DNo');
        input2.attr('name', 'cheqmodeldep[' + i + '].ChequeNo');

        var input3 = $(this).find('.ISdate');
        input3.attr('name', 'cheqmodeldep[' + i + '].Date');

        var input6 = $(this).find('.Attach');
        input6.attr('name', 'cheqmodeldep[' + i + '].Attachments');

        var input7 = $(this).find('.BankName');
        input7.attr('name', 'cheqmodeldep[' + i + '].Bank');

        var dltbtn = $(this).find('.Dep-ed-dlt');
        var addbtn = $(this).find('.Dep-ed-add');
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
function deleteRowDep(t, arg,id) {
    var classname = $(t).closest('div').attr('class');
    if (Dchecount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        if (id > 0) {
            var url = "/Property/PropertyRegistration/Deletecheque/" + id;
            var data = $('#deleteform').serialize();
            $.ajax({
                type: "POST",
                url: url,
                data: data,
                success: function (data) {
                    if (data.status) {


                     
                        var e = t.parentNode.parentNode.parentNode;
                        e.parentNode.removeChild(e);
                        Dchecount--;
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
            Dchecount--;
        }
    }
    resetDocbtnDep();
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
function searchBankNameDep() {

    $(".DepBankName").select2({
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
function deptypechange(selectedObject, dataid) {
    searchBankNameDep();
}
function AddDep() {
    AddDepCheq();
    searchBankNameDep();
}
