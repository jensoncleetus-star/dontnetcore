var edcount = 0;
var procount = 0;
var doccount = 0;

function AddEducation(Course, Spec, Inst,Uni,Year,Per) {
    var previouscount = parseFloat(edcount) - 1;
    var previous = $('#edu_' + previouscount).val();
    var req = "";
    var payroll = $("#chkpayroll").val();
    if (payroll == 'active')
    {
       // req = 'required = "required"';
    }

    if (edcount == 0 || previous != ""  ) {
        var edumod = $("input[name='edumodel[" + previouscount + "].Course']").val();

        Course = (Course == undefined) ? "" : Course;
        Spec = (Spec == undefined) ? "" : Spec;
        Inst = (Inst == undefined) ? "" : Inst;
        Uni = (Uni == undefined) ? "" : Uni;
        Year = (Year == undefined) ? "" : Year;
        Per = (Per == undefined) ? "" : Per;
        if (edcount == 0 || edumod != "") {
            var html = '<div class="eduSet">' +
            '<div class="row">' +
            '<div class="form-group col-md-6"><label class="control-label">Course</label><input class="form-control Crs" value="' + Course + '" id="course_' + edcount + '" name="edumodel[].Course" type="text" placeholder="Enter Course" ' + req + '/></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Specialization</label><input class="form-control Spec" value="' + Spec + '" id="spec_' + edcount + '" name="edumodel[].Specialization" type="text" placeholder="Enter Specialization" /></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Institute</label><input class="form-control Ins" value="' + Inst + '" id="instit_' + edcount + '" name="edumodel[].Institute" type="text" placeholder="Enter Institute" /></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Board/University</label><input class="form-control Uni" value="' + Uni + '" id="univer_' + edcount + '" name="edumodel[].University" type="text" placeholder="Enter Board/University" /></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Passing Year</label> <div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker eduyear Yea" value="' + Year + '" id="year_' + edcount + '" name="edumodel[].PassingYear" type="text" placeholder="Enter Passing Year" /></div></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Percentage</label><input type="number" class="form-control Per" value="' + Per + '" id="percent_' + edcount + '" name="edumodel[].Percentage" type="text" placeholder="Enter Percentage" /></div>' +
            '<div class="form-group col-md-12 text-right"><span class="input-group-btn ed-add"><button type="button" class="btn btn-flat btn-success"  onclick="AddEducation()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn ed-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowED(this,' + edcount + ')">Delete <i class="fa fa-trash"></i></button></span></div>' +
            '</div>'
            $(html).appendTo($("#EduDetails"));
            edcount++;
            resetEDbtn();


            $(".eduyear").datepicker({
                format: "yyyy",
                viewMode: "years",
                minViewMode: "years",
                autoclose: true,
            });
            jQuery.validator.methods["date"] = function (value, element) { return true; }
        } else {
            $("input[name='edumodel[" + previouscount + "].Course']").focus();
        }
    }   
}
function resetEDbtn() {
    var i = 0;
    var mbLen = $(".eduSet .row").length;

    $('.eduSet .row').each(function (index, element) {
        var input1 = $(this).find('.Crs')
        input1.attr('name', 'edumodel[' + i + '].Course');

        var input2 = $(this).find('.Spec');
        input2.attr('name', 'edumodel[' + i + '].Specialization');

        var input3 = $(this).find('.Ins');
        input3.attr('name', 'edumodel[' + i + '].Institute');

        var input4 = $(this).find('.Uni');
        input4.attr('name', 'edumodel[' + i + '].University');

        var input5 = $(this).find('.Yea');
        input5.attr('name', 'edumodel[' + i + '].PassingYear');

        var input6 = $(this).find('.Per');
        input6.attr('name', 'edumodel[' + i + '].Percentage');

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
function deleteRowED(t, arg) {
    var classname = $(t).closest('div').attr('class');
    if (edcount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode.parentNode;
        e.parentNode.removeChild(e);
        edcount--;
    }
    resetEDbtn();
}

function AddProfession(Org,Desg,FD,TD,Res,Skil,Reason) {
    var previouscount = parseFloat(procount) - 1;
    var previous = $('#pro_' + previouscount).val();

    var req = "";
    var payroll = $("#chkpayroll").val();
    if (payroll == 'active') {
        //req = 'required = "required"';
    }
    if (procount == 0 || previous != "") {
        var promod = $("input[name='promodel[" + previouscount + "].Organization']").val();

        Org = (Org == undefined) ? "" : Org;
        Desg = (Desg == undefined) ? "" : Desg;
        FD = (FD == undefined) ? "" : convertToDate(FD);
        TD = (TD == undefined) ? "" : convertToDate(TD);
        Res = (Res == undefined) ? "" : Res;
        Skil = (Skil == undefined) ? "" : Skil;
        Reason = (Reason == undefined) ? "" : Reason;
        if (procount == 0 || promod != "") {
            var html = '<div class="proSet">' +
            '<div class="row">' +
            '<div class="form-group col-md-6"><label class="control-label">Organization</label><input class="form-control Org" value="' + Org + '" id="org_' + procount + '" name="promodel[].Organization" type="text" placeholder="Enter Organization Name" ' + req + '/></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Designation</label><input class="form-control Dsg" value="' + Desg + '" id="dsg_' + procount + '" name="promodel[].Designation" type="text" placeholder="Enter Designation Name" /></div>' +
            '<div class="form-group col-md-6"><label class="control-label">From Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker fromd Fdate" value="' + FD + '" id="fdate_' + procount + '" name="edumodel[].FromDate" type="text" placeholder="Enter From Date" /></div></div>' +
            '<div class="form-group col-md-6"><label class="control-label">To Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker tod Tdate" value="' + TD + '" id="tdate_' + procount + '" name="edumodel[].ToDate" type="text" placeholder="Enter To Date" /></div></div>' +
            '<div class="form-group col-md-6" hidden><label class="control-label">Responsibility</label><input class="form-control Res" value="' + Res + '" id="res_' + procount + '" name="promodel[].Responsibility" type="text" placeholder="Enter Responsibility" /></div>' +
            '<div class="form-group col-md-6" hidden><label class="control-label">Skills</label><input class="form-control Skil" value="' + Skil + '" id="skil_' + procount + '" name="promodel[].Skills" type="text" placeholder="Enter Skills" /></div>' +
           
            '<div class="form-group col-md-12 text-right"><span class="input-group-btn ed-add"><button type="button" class="btn btn-flat btn-success"  onclick="AddProfession()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn ed-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowPro(this,' + procount + ')">Delete <i class="fa fa-trash"></i></button></span></div>' +

            '</div>'
            $(html).appendTo($("#ProDetails"));
            procount++;
            resetProbtn();


            $(".fromd").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });
            $(".tod").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });
            jQuery.validator.methods["date"] = function (value, element) { return true; }
        } else {
            $("input[name='promodel[" + previouscount + "].Organization']").focus();
        }
    }
}
function resetProbtn() {
    var i = 0;
    var mbLen = $(".proSet .row").length;

    $('.proSet .row').each(function (index, element) {
        var input1 = $(this).find('.Org');
        input1.attr('name', 'promodel[' + i + '].Organization');

        var input2 = $(this).find('.Dsg');
        input2.attr('name', 'promodel[' + i + '].Designation');

        var input3 = $(this).find('.Fdate');
        input3.attr('name', 'promodel[' + i + '].FromDate');

        var input4 = $(this).find('.Tdate');
        input4.attr('name', 'promodel[' + i + '].ToDate');

        var input5 = $(this).find('.Res');
        input5.attr('name', 'promodel[' + i + '].Responsibility');

        var input6 = $(this).find('.Skil');
        input6.attr('name', 'promodel[' + i + '].Skills');
       

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
function deleteRowPro(t, arg) {
    var classname = $(t).closest('div').attr('class');
    if (procount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode.parentNode;
        e.parentNode.removeChild(e);
        procount--;
    }
    resetProbtn();
}


function AddDocument(Doc, No, ID, ED, Note, AttAch, PersonalNo, EmployeeDocumentId) {
    var previouscount = parseFloat(doccount) - 1;
    var previous = $('#doc_' + previouscount).val();
    var req = "";
    var payroll = $("#chkpayroll").val();
    if (payroll == 'active') {
        //req = 'required = "required"';
    }
    if (doccount == 0 || previous != "") {
        var docmod = $("input[name='docmodel[" + previouscount + "].DocumentName']").val();

        Doc = (Doc == undefined) ? "" : "<option value='" + Doc + "'>" + Doc + "</option>";

        No = (No == undefined) ? "" : No;
        PersonalNo = (PersonalNo == undefined) ? "" : PersonalNo;
        ID = (ID == undefined) ? "" : convertToDate(ID);
        ED = (ED == undefined) ? "" : convertToDate(ED); 
        Note = (Note == undefined) ? "" : Note;

        AttAch = (AttAch == undefined) ? "" : AttAch;
        EmployeeDocumentId = (EmployeeDocumentId == undefined) ? 0 : EmployeeDocumentId;
        var attachimage = "";
        var empid = $("#id").val();
        if (AttAch != "") {
            attachimage = '<a   href="/uploads/empdocuments/' + empid + '/' + AttAch + '" target="/new/">' + AttAch + ' </a>'
        }
        if (doccount == 0 || docmod != "") {
            var html = '<div class="docSet">' +
            '<div class="row">' +
            '<div class="form-group col-md-6 select2-box"><label class="control-label">Document Name</label><select class="form-control DName" id="dname_' + doccount + '" name="docmodel[].DocumentName" type="text" placeholder="Enter Document Name" ' + req + '>' + Doc + '</select></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Document No</label><input class="form-control DNo" value="' + No + '" id="dnum_' + doccount + '" name="docmodel[].DocumentNo" type="text" placeholder="Enter Document No" /></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Personal No</label><input class="form-control PerNo" value="' + PersonalNo + '" id="dperno_' + doccount + '" name="docmodel[].PersonalNo" type="text" placeholder="Enter Personal No" /></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Issue Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker issd ISdate" value="' + ID + '" id="issdate_' + doccount + '" name="docmodel[].IssueDate" type="text" placeholder="Enter Issue Date" /></div></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Expiry Date</label><div class="input-group date"><div class="input-group-addon"><i class="fa fa-calendar"></i></div><input class="form-control datepicker expd EXdate" value="' + ED + '" id="expdate_' + doccount + '" name="docmodel[].ExpiryDate" type="text" placeholder="Enter Exp Date" /></div></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Note</label><textarea class="form-control Note" id="dnote_' + doccount + '" name="docmodel[].Note" type="text" placeholder="Enter Note" >' + Note + '</textarea></div>' +
            '<div class="form-group col-md-6"><label class="control-label">Attachments</label><input type="file" class="form-control Attach" value="' + AttAch + '" id="dattach_' + doccount + '" name="docmodel[].Attachments" type="text" placeholder="Enter Attachments" />' + attachimage + '</div>' +
            '<div class="form-group col-md-12 text-right"><span class="input-group-btn ed-add"><button type="button" class="btn btn-flat btn-success"  onclick="AddDocument()">Add <i class="fa fa-plus"></i></button></span> &nbsp <span class="input-group-btn ed-dlt hide"><button type="button" class="btn btn-flat btn-danger"  onclick="deleteRowDoc(this,' + doccount + ')">Delete <i class="fa fa-trash"></i></button></span></div>' +
                '<input class="inputdocid" value="' + EmployeeDocumentId + '" id="EmployeeDocumentId_' + doccount + '" name="docmodel[].EmployeeDocumentId" type="hidden"  />' 
            '</div>'
            $(html).appendTo($("#DocDetails"));
            doccount++;
            resetDocbtn();
            bindDoc();

            $(".issd").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });
            $(".expd").datepicker({
                format: 'dd-mm-yyyy',
                autoclose: true,
                allowInputToggle: true
            });
            jQuery.validator.methods["date"] = function (value, element) { return true; }
        } else {
            $("input[name='docmodel[" + previouscount + "].DocumentName']").focus();
        }
    }
}
function bindDoc(){
    $(".DName").select2({
        tags: true,
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeDoc",
            dataType: 'json',
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
function resetDocbtn() {
    var i = 0;
    var mbLen = $(".docSet .row").length;

    $('.docSet .row').each(function (index, element) {
        var input1 = $(this).find('.DName');
        input1.attr('name', 'docmodel[' + i + '].DocumentName');
        
        input1.attr('name', 'docmodel[' + i + '].DocumentName');
        var inputdocid = $(this).find('.inputdocid');
        inputdocid.attr('name', 'docmodel[' + i + '].EmployeeDocumentId');

        var input2 = $(this).find('.DNo');
        input2.attr('name', 'docmodel[' + i + '].DocumentNo');

        var input3 = $(this).find('.ISdate');
        input3.attr('name', 'docmodel[' + i + '].IssueDate');

        var input4 = $(this).find('.EXdate');
        input4.attr('name', 'docmodel[' + i + '].ExpiryDate');

        var input5 = $(this).find('.Note');
        input5.attr('name', 'docmodel[' + i + '].Note');

        var input6 = $(this).find('.Attach');
        input6.attr('name', 'docmodel[' + i + '].Attachments');

        var input7 = $(this).find('.PerNo');
        input7.attr('name', 'docmodel[' + i + '].PersonalNo');




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
function deleteRowDoc(t, arg) {
    var classname = $(t).closest('div').attr('class');
    if (doccount == 1) alert("Sorry You Can't Delete This Row.");
    else {
        var e = t.parentNode.parentNode.parentNode;
        e.parentNode.removeChild(e);
        doccount--;
    }
    resetDocbtn();
}