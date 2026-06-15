var count = 1, type = '';
limits = 500;
function addempitem(t, action, EmpName, EmpId, AtType, AtTypeId, Value, Unit, EmpCode, CBalance, Item) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option1 = "";
        var Option2 = "";
        var readonly = "";
        var row = "";
        if (EmpId != null) {
            row = "<tr class='emp_" + EmpId + "' id='emp_" + count + "'>";
        } else {
            row = "<tr class='emp_' id='emp_" + count + "'>";
        }

        var slno = $('#addempitem tr').length + 1;
        
        tabindex = count * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        tab4 = tabindex + 4;
        tab5 = tabindex + 5;
        tab6 = tabindex + 6;
        if (Item != null) {
            // row = "<tr class='item_" + Item + "' id='item_" + count + "'>";
            Option1 = "<option value='" + EmpId + "'>" + EmpName + "</option>";
            Option2 = "<option value='" + AtTypeId + "'>" + AtType + "</option>";
        } else {
            if (EmpName != "") {
                Option1 = "<option value='" + 0 + "'>" + EmpName + "</option>";
            }
            if (AtType != "") {
                Option2 = "<option value='" + 0 + "'>" + AtType + "</option>";
            }
        }
        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }

        var attaddbtn = "<span class='input-group-btn'><a type='button' href='/Hr/AttendanceType/Create' class='modal-create btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";


        EmpCode = EmpCode != null ? EmpCode : "";
        data = "<td class='text-center'> " + slno + " </td>" +
               "<td width='250px'><select data-name='EmployeeId' class='form-control ddlEmployee' " + required + " data-id='" + count + "' placeholder='Employee Name' id='employeeId_" + count + "' data-msg-required='Employee is required' onchange='GetEmpDetails(this," + count + ",\"" + type + "\")'>" + Option1 + "</select></td>" +
               "<td><input type='text' data-name='EmpCode' name='EmpCode[]' id='empcode_" + count + "' value='" + EmpCode + "'  class='empcode_" + count + " form-control text-center empcode'  tabindex='" + tab2 + "' readonly='readonly'/></td>" +
               "<td class='input-group input-group-sm' width='200px'><select data-name='AttendanceType' class='form-control AtType' " + required + " data-id='" + count + "' placeholder='Attenedence Type' id='attenedencetype_" + count + "' data-msg-required='Attenedence Type is required' onchange='GetAttypechange(this," + count + ",\"" + type + "\")'>" + Option2 + "</select>" + attaddbtn + "</td>" +
               "<td><input type='text' data-name='CBalance' name='CBalance[]' id='cbalance_" + count + "' value=''  class='cbalance_" + count + " form-control text-right cbalance'  tabindex='" + tab4 + "' readonly='readonly'/></td>" +
               "<td><input type='number' data-name='Value' name='Value[]' id='value_" + count + "' value='"+Value+"'  class='value_" + count + " form-control text-right value' placeholder='0' value='0' min='.5' tabindex='" + tab5 + "'/></td>" +
               "<td><input type='text' data-name='Unit' name='Unit[]' id='unit_" + count + "' value='"+ Unit +"'  class='unit_" + count + " form-control text-center unit'  tabindex='" + tab6 + "' readonly='readonly'/></td>" +
               "<td class='text-center'><input type='hidden' data-name='EmpName' class='empname_" + count + "' value='" + EmpName + "' name='empname' id='empname_" + count + "'/><input type='hidden' data-name='ATypeName' class='atype_" + count + "' value='" + AtType + "' name='atype' id='atype_" + count + "'/><button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        searchEmployee();
        searchAtType();

        count++;
        setTabIndex();
    }
}
function searchEmployee() {
    var selecteditem = new Array();
    $(".ddlEmployee").each(function () {
        selecteditem.push($(this).val());
    });

    $(".ddlEmployee").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Employee/SearchEmployeeWithAcc",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"
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
    });
}
function searchAtType() {
    var selecteditem = new Array();
    $(".AtType").each(function () {
        selecteditem.push($(this).val());
    });

    $(".AtType").select2({
        placeholder: 'Search by Name',
        minimumInputLength: 0,
        ajax: {
            url: "/AttendanceType/SearchAttendanceType",
            dataType: 'json',
            delay: 50,
            data: function (params) {
                return {
                    q: params.term || "",
                    page: params.page || 0,
                    x: "empty"

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
    });
}

//item details
function GetEmpDetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var EmpId = selectObject.value;
        if (EmpId != null) {
            if ($(".emp_" + EmpId).length > 0) {
                alert("Sorry You Cant Add An Employee More Than One Time..!!");
                $(selectObject).val(null).trigger('change');
            }
            else {
                $(selectObject).closest('tr').attr('class', "emp_" + EmpId);
                addempitem('addempitem', "", "", "", "", "", "", "", "");
                itemUpdate(selectObject, dataid, action);
            }
        }
    }
}
function itemUpdate(selectObject, dataid, action) {

    $.ajax({
        url: '/Employee/GetEmployeeById',
        type: "GET",
        dataType: "JSON",
        data: { empID: selectObject.value },
        success: function (result) {
            $(".empcode_" + dataid).val(result.EMPCode);
        }
    });

}
function GetAttypechange(selectObject, dataid, action) {
    $.ajax({
        url: '/AttendanceType/AttendanceTypeById',
        type: "GET",
        dataType: "JSON",
        data: { typeID: selectObject.value },
        success: function (result) {
            $(".unit_" + dataid).val(result.Unit);
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
        if ($(this).closest("tr").hasClass("item_") && !$(this).hasClass("select2-selection__rendered")) {
            $(this).attr('tabindex', -1);
        }
    });
}
//Delete a row of table
function deleteRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'emp_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
        var e = t.parentNode.parentNode;
        e.parentNode.removeChild(e);
        }
    }
    var i = 1;
    $('#addempitem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}

function AttendanceSubmit(fnval) {
    var HTMLtbl = {
        getData: function (table) {
            var data = [];
                table.find('tr').not(':first').not('.item_').each(function (rowIndex, r) {
                var cols = {};
                $(this).find('input,select').each(function (colIndex, c) {
                    itid = $(this).attr('data-name').split(' ')[0];
                    itval = ($(this).val() != "") ? $(this).val() : $(this).text();
                    cols[itid] = itval;
                });
                data.push(cols);
            });
            return data;
        }
    }

    var data = HTMLtbl.getData($('#normalinvoice'));
    var parameters = {};
    parameters.empitems = data;
    parameters.AttendanceId = $('#AttendanceId').val();
    parameters.VoucherNo = $('#VoucherNo').val();
    parameters.AtDate = $('#AtDate').val();
    parameters.Note = $('#Note').val();
    parameters.Remarks = $('#Remarks').val();
    parameters.Branch = $('#ddlBranch').val();

    var url = "";
    if (fnval == "save") {
        url = "/Hr/Attendance/CreateAttendance";
    }
    if (fnval == "update") {
        url = "/Hr/Attendance/EditAttendance";
    }

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
        success: function (e) {
            $('.ajax_response', res_success).text(e.message);
            $('.AlertDiv').prepend(res_success);
            if (fnval != null) {
                window.location.href = '/Hr/Attendance/Index';
            } else {
                location.reload();
            }
            $("button").prop('disabled', false); // enable button
        }
    });
}
