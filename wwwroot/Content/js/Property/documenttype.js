var Dcount = 0;
function Adddoctype(ID,type, AttAch, typeval, Id) {
    var previouscount = parseFloat(Dcount) - 1;
    var previous = $('#doc_' + previouscount).val();
    var req = "";
    var payroll = $("#chkpayroll").val();
    if (payroll == 'active') {
        req = 'required = "required"';
    }
    searchDocType();
    var Option = "";
    if (Dcount == 0 || previous != "") {
        var docmod = $("input[name='docmodel[" + previouscount + "].Type']").val();

        type = (type == undefined) ? "" : type;
        if (type != null) {
            Option = "<option value='" + typeval + "'>"+type+"</option>";
        }
        ID = (ID == undefined) ? 0 : ID;
        AttAch = (AttAch == undefined) ? "" : AttAch;
        var attachimage = "";
        var empid = $("#id").val();
        if (AttAch != "") {
            attachimage = '<a  alt=' + AttAch + ' href="/uploads/TenancyContractDocument / ' + Id + ' / ' + AttAch + '"  target="_new">' + AttAch + '</a>'

        }
        searchDocType();
        if (Dcount == 0 || docmod != "") {
            
            var html = '<div class="docmntSet">' +
                '<div class="row">' +
                '<div class="form-group"><input  value="' + ID + '" id="docmodel' + Dcount + '" name="docmodel[' + Dcount + '].ID"  value="' + ID + '" type="hidden" /></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Type</label><select class="form-control DocName" onclick="typechange(this,' + Dcount + ');" value="' + type + '" id="DocName_' + Dcount + '" name="docmodel[' + Dcount + '].Type" type="text" placeholder="Enter Document Name" ' + req + '>'+Option+'</select></div>' +
            '<div class="form-group col-md-2"><label class="control-label">Attachments</label><input type="file" class="form-control Attache" value="' + AttAch + '" id="dattach_' + Dcount + '" name="docmodel[' + Dcount + '].Attachments" type="text" placeholder="Enter Attachments" />' + attachimage + '</div>' +
            '<div class="form-group col-md-6 text-right"><span class="input-group-btn edoc-add"><button type="button" class="btn btn-flat btn-success"  onclick="Adddoctype()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn edoc-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowtype(this,' + Dcount + ')">Delete <i class="fa fa-trash"></i></button></span></div>' +
            '</div>'
            $(html).appendTo($("#DocDetails"));
            Dcount++;
            resetypebtn();

        }
    }
}

function searchDocType() {
    var selecteditem = new Array();

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
                        x: "all"

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
function deleteRowtype(t, arg) {
    var classname = $(t).closest('div').attr('class');
    if (Dcount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode.parentNode;
        e.parentNode.removeChild(e);
        Dcount--;
    }
    resetypebtn();
}