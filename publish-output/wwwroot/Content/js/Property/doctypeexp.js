var Doccount = 0;


function Adddoctypeexp2(ID, type, AttAch, typeval, Id, Date, Purpose) {
    var previouscount = parseFloat(Doccount) - 1;
    var previous = $('#issdate_' + previouscount).val();
    var req = "";
    var payroll = $("#chkpayroll").val();
    if (payroll == 'active') {
        req = 'required = "required"';
    }
    ID = (ID == undefined) ? 0 : ID;
    type = (type == undefined) ? "" : type;
    typeval = (typeval == undefined) ? "" : typeval;
    Date = ((Date == undefined) || (Date == null) || (Date == "")) ? "" : convertToDate(Date);

    var Option = "";
    if (1) {
        var docmod = $("input[name='docmodel[" + previouscount + "].Type']").val();

        var Enddat = $('#EndDate').val();
        if (Enddat != undefined && Enddat != '') {
            // Date = $('#EndDate').val();
        }
        //Date = (Date == undefined) ? "" : (Date);


        if (type != null) {
            Option = "<option value='" + typeval + "'>" + type + "</option>";
        }
        //var readtype = (Purpose == 'TenancyContract') || (Enddat !=undefined) ? 'disabled' : '';

        AttAch = (AttAch == undefined) ? "" : AttAch;
        var attachimage = "";
        var empid = $("#id").val();
        if (AttAch != "") {
            
           // attachimage = '<a  alt=' + AttAch + ' href="/uploads/PropertyContractDocument/Property_" + Id + '/' + AttAch + '"  target="_new">' + AttAch + '</a>'
            attachimage = '<a  alt=' + AttAch + ' href="/uploads/PropertyContractDocument/Property_'  + Id + '/' + AttAch + '"  target="_new">' + AttAch + '</a>'

        }
        //searchDocType();
        if (Doccount == 0 || docmod != "") {

            var html = '<div class="docmntSet">' +
                '<div class="row">' +
                //'<div class="form-group col-md-2"><label class="control-label">Document</label><select class="form-control DocName"  onclick="typechange(this,' + Doccount + ');" value="' + type + '" id="DocName_' + Doccount + '" name="docmodel[' + Doccount + '].Type" type="text" placeholder="Enter Document Name" ' + req + '>' + Option + '</select></div>' +
                '<div class="form-group"><input  value="' + ID + '" id="docmodel' + Doccount + '" name="docmodel[' + Doccount + '].ID"  value="' + ID + '" type="hidden" /></div>' +

                '<div class="form-group col-md-2"><label class="control-label">Type</label><select class="form-control DocName" onclick="searchDocType();" value="' + type + '" id="doc_' + Doccount + '" name="docmodel[' + Doccount + '].Type" type="text" placeholder="Enter Document Name" ' + req + '>' + Option + '</select></div>' +
                '<div class="form-group col-md-2"><label class="control-label">Expiry Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker issd ISdate"  value="' + Date + '" id="issdate_' + Doccount + '" name="docmodel[' + Doccount + '].Date" type="text" placeholder="Enter Date" /></div></div>' +
                '<div class="form-group col-md-2"><label class="control-label">Attachments</label><input type="file" class="form-control Attache" value="' + AttAch + '" id="dattach_' + Doccount + '" name="docmodel[' + Doccount + '].Attachments" type="text" placeholder="Enter Attachments" />' + attachimage + '</div>' +
                '<div class="form-group col-md-1 text-right"><label class="control-label"></label><span class="input-group-btn edoc-add"><button type="button" class="btn btn-flat btn-success"  onclick="Adddoctypeexp()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn edoc-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowtype(this,' + Doccount + ',' + ID + ')">Delete <i class="fa fa-trash"></i></button></span></div>';
            $(html).appendTo($("#DocDetails"));
            Doccount++;
            resetypebtn();

            $(".issd").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });

            jQuery.validator.methods["date"] = function (value, element) { return true; }
        }
    }
}
function Adddoctypeexp(ID,type, AttAch, typeval, Id, Date, Purpose) {
    var previouscount = parseFloat(Doccount) - 1;
    var previous = $('#doc_' + previouscount).val();
    var req = "";
    var payroll = $("#chkpayroll").val();
    if (payroll == 'active') {
        req = 'required = "required"';
    }
    ID = (ID == undefined) ? 0 : ID;
	type = (type == undefined) ? "" : type;
    typeval = (typeval == undefined) ? "" : typeval;
   Date = ((Date == undefined) || (Date == null) || (Date == "")) ? "" : convertToDate(Date);
   
    var Option = "";
    if (Doccount == 0 || (previous != "")) {
        var docmod = $("input[name='docmodel[" + previouscount + "].Type']").val();
        
        var Enddat = $('#EndDate').val();
        if (Enddat != undefined && Enddat != '') {
           // Date = $('#EndDate').val();
        }
        //Date = (Date == undefined) ? "" : (Date);

        
        if (type != null) {
            Option = "<option value='" + typeval + "'>" + type + "</option>";
        }
        //var readtype = (Purpose == 'TenancyContract') || (Enddat !=undefined) ? 'disabled' : '';

        AttAch = (AttAch == undefined) ? "" : AttAch;
        var attachimage = "";
        var empid = $("#id").val();
        if (AttAch != "") {
            //attachimage = '<img class="img-responsive editclogo" alt=' + AttAch + ' src="/uploads/TenancyContractDocument/' + Purpose + Id + '/' + AttAch + '" />'
            attachimage = '<a  alt=' + AttAch + ' href="/uploads/TenancyContractDocument/' + Purpose + Id + '/' + AttAch+ '"  target="_new">' + AttAch + '</a>'

        }
        //searchDocType();
        if (Doccount == 0 || docmod != "") {

            var html = '<div class="docmntSet">' +
            '<div class="row">' +
            //'<div class="form-group col-md-2"><label class="control-label">Document</label><select class="form-control DocName"  onclick="typechange(this,' + Doccount + ');" value="' + type + '" id="DocName_' + Doccount + '" name="docmodel[' + Doccount + '].Type" type="text" placeholder="Enter Document Name" ' + req + '>' + Option + '</select></div>' +
                '<div class="form-group"><input  value="' + ID + '" id="docmodel' + Doccount + '" name="docmodel[' + Doccount + '].ID"  value="' + ID + '" type="hidden" /></div>' +

                '<div class="form-group col-md-2"><label class="control-label">Type</label><select class="form-control DocName" onclick="searchDocType();" value="' + type + '" id="doc_' + Doccount + '" name="docmodel[' + Doccount + '].Type" type="text" placeholder="Enter Document Name" ' + req + '>' + Option + '</select></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Expiry Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker issd ISdate"  value="' + Date + '" id="issdate_' + Doccount + '" name="docmodel[' + Doccount + '].Date" type="text" placeholder="Enter Date" /></div></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Attachments</label><input type="file" class="form-control Attache" value="' + AttAch + '" id="dattach_' + Doccount + '" name="docmodel[' + Doccount + '].Attachments" type="text" placeholder="Enter Attachments" />' + attachimage + '</div>' +
                '<div class="form-group col-md-1 text-right"><label class="control-label"></label><span class="input-group-btn edoc-add"><button type="button" class="btn btn-flat btn-success"  onclick="Adddoctypeexp()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn edoc-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowtype(this,' + Doccount + ','+ID+ ')">Delete <i class="fa fa-trash"></i></button></span></div>';
            $(html).appendTo($("#DocDetails"));
            Doccount++;
            resetypebtn();

            $(".issd").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });

            jQuery.validator.methods["date"] = function (value, element) { return true; }
        }
    }
}

function searchDocType() {
    var selecteditem = new Array();
    var section = $('#Section').val();
    $(".DocName").each(function () {
        selecteditem.push($(this).val());
    });

    $(".DocName").select2({
        placeholder: 'Search Document Type',
        minimumInputLength: 0,
        ajax: {
            url: "/Property/DocumentType/SearchDocumentType",
            dataType: 'json',
            type: "POST",
            delay: 50,
            data: function (params) {
                return {
                    q: params.term,
                    page: params.page,
                    x: "all",
                    Sectn: section
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
    searchDocType();
}



function resetypebtn() {
    var i = 0;
    var mbLen = $(".docmntSet .row").length;

    $('.docmntSet .row').each(function (index, element) {
        var input1 = $(this).find('.DocName');
        input1.attr('name', 'docmodel[' + i + '].Type');

        var input6 = $(this).find('.Attache');
        input6.attr('name', 'docmodel[' + i + '].Attachments');

        var input3 = $(this).find('.ISdate');
        input3.attr('name', 'docmodel[' + i + '].Date');

        var dltbtn = $(this).find('.edoc-dlt');
        var addbtn = $(this).find('.edoc-add');
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
function deleteRowtype(t, arg,id) {
    var classname = $(t).closest('div').attr('class');
    if (Doccount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        
        if (id > 0) {

            var url = "/Property/PropertyRegistration/Deletetenentdoc/" + id;
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
                        Doccount--;
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
            Doccount--;
        }
    }
    resetypebtn();
}