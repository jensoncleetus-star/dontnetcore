var count = 1, countd = 1, type = '';
limits = 500;
function addattendance(t, action, Date, AtTypeId, AtType, OverTime, EmpId, Holiday) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option1 = "";
        var readonly = "";
        var row = "";
        if (Date != null) {
            row = "<tr class='dailyat da_" + count + "' id='da_" + count + "'>";
        } else {
            row = "<tr class='dailyat da_' id='da_" + count + "'>";
        }

        var slno = $('#addattendance tr').length + 1;

        tabindex = count * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;

        if (AtTypeId != null && AtTypeId != "") {
            Option1 = "<option value='" + AtTypeId + "'>" + AtType + "</option>";
        }
            //else if (Holiday == true) {
            //    Option1 = "<option value='1' selected='selected'>ABSENT</option>";
            //}
        else {

            if (Holiday == true) {
                Option1 = "<option value='5' selected='selected'>HOLIDAY</option>";
            } else {
                Option1 = "<option value='4' selected='selected'>PRESENT</option>";
            }
        }
        var required = "";
        if (count == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }
        var htype = "";
        var hdis = "";
        if (Holiday == true) {
           // hdis = "disabled='disabled'";
            htype = "readonly = 'readonly'";
        }

        OverTime = OverTime != null ? OverTime : 0;
        Date = Date != null ? convertToDate(Date) : 0;

        data = "<td class='text-center'><input type='text' name='addattendance[].AtDate' id='date_" + count + "' value='" + Date + "'  class='date_" + count + " form-control text-center seldate'  tabindex='" + tab1 + "' readonly='readonly'/></td>" +
               "<td class='input-group input-group-sm' width='100%'><select  name='addattendance[].AtType' data-name='AttendanceType' class='form-control AtType' " + required + " data-id='" + count + "' tabindex='" + tab2 + "' " + htype + " onchange='GetAttypechange(this," + count + ")' id='attenedencetype_" + count + "' " + hdis + ">" + Option1 + "</select></td>" +
               "<td><input type='number' name='addattendance[].OverTime' id='overtime_" + count + "' value='" + OverTime + "'  class='overtime_" + count + " form-control text-right OverTime' tabindex='" + tab3 + "' onchange='overtime_change();'/>" +
               "<input type='hidden' name='addattendance[].EmployeeId' id='empid_" + count + "' value='" + EmpId + "'  class='empid_" + count + " form-control text-center EmpId' /> ";
        "</td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        resetStrBtn();
        searchAtType();
        count++;
        setTabIndex();
    }
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
                    //x: "empty"

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

function resetStrBtn() {
    var i = 0;
    $('.dailyat').each(function (index, element) {
        var input1 = $(this).find('.seldate')
        input1.attr('name', 'addattendance[' + i + '].AtDate');

        var input2 = $(this).find('.AtType');
        input2.attr('name', 'addattendance[' + i + '].AtType');

        var input3 = $(this).find('.OverTime');
        input3.attr('name', 'addattendance[' + i + '].OverTime');

        var input4 = $(this).find('.EmpId');
        input4.attr('name', 'addattendance[' + i + '].EmployeeId');
        i++;
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

function bindAttendance() {
    var Emp = $("#ddlEmployee").val();
    var MonthYear = $("#MonthYear").val();
    if (Emp != "" && MonthYear != "") {
        $.ajax({
            url: '/Hr/DailyAttendance/GetDailyAttendance',
            dataType: 'json',
            data: { Emp: Emp, MonthYear: MonthYear },
            cache: true,
            success: function (data) {
                if (data != null) {
                    if (data == 0) {
                        alert("Selected Date Should greater than Employee's Join Date ..!!");
                    } else {
                        $('#addattendance').html('');
                       
                        $.each(data, function (i, item) {
                            $("#DailyAttendanceId").val(item.DailyAttendanceId);
                            addattendance('addattendance', '', item.Date, item.AtTypeId, item.AtType, item.OverTime, item.EmpId, item.Holiday);
                        });
                        $("#imgload").hide();
                        $(".attdiv").show();
                        $(".deptattdiv").hide();
                        $(".btndiv").show();

                        overtime_change();
                        GetAttypeTotal();
                    }
                }
            }
        });

    }
}

function overtime_change() {
    var OverTime = 0;
    $(".OverTime").each(function () {
        var indVal = $(this).val();
        OverTime = parseFloat(OverTime) + parseFloat(indVal);
    });
    $("#TotOverTime").val(OverTime.toFixed(2));
}
function GetAttypechange(selectObject, dataid) {
    GetAttypeTotal();
}

function GetAttypeTotal() {
    var tbody = $("#normalinvoice tbody");
    if (tbody.children().length > 0) {
        $(".attypesel").each(function () {
            var attVal = $(this).attr("id");
            var arr = attVal.split('_');
            var AbCount = 0;
            $(".AtType").each(function () {
                var SelVal = $(this).val();
                if (arr[1] == SelVal) {
                    AbCount++;
                    $("#Att_" + SelVal).val(AbCount);
                }
            });
        });
    }

    $("#AtTotalDay").val(tbody.children().length);
}

function bindDeptAttendance() {
    var Dept = $("#ddlDepartment").val();
    var AtDate = $("#AtDate").val();
    if (Dept != "" && AtDate != "") {
        $.ajax({
            url: '/Hr/DailyAttendance/GetDeptDailyAttendance',
            dataType: 'json',
            data: { Dept: Dept, AtDate: AtDate },
            cache: true,
            success: function (data) {
                if (data != null) {
                    $('#deptaddattendance').html('');
                    $.each(data, function (i, item) {
                        $("#DailyAttendanceId").val(item.DailyAttendanceId);
                        deptaddattendance('deptaddattendance', '', item.EmpId, item.EmpName, item.Date, item.AtTypeId, item.AtType, item.OverTime, item.Holiday);
                    });
                    $("#imgload").hide();
                    $(".attdiv").hide();
                    $(".deptattdiv").show();
                    $(".deptattdiv").show();
                    $(".btndiv").show();
                }
                else {
                    alert("Not found..!!");
                }
            }
        });
    }
}

function deptaddattendance(t, action,EmpId,EmpName, Date, AtTypeId, AtType, OverTime, Holiday) {
    if (countd == limits) alert("You have reached the limit of adding " + countd + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option1 = "";
        var readonly = "";
        var row = "";
        if (Date != null) {
            row = "<tr class='dailyat da_" + countd + "' id='da_" + countd + "'>";
        } else {
            row = "<tr class='dailyat da_' id='da_" + countd + "'>";
        }

        var slno = $('#deptaddattendance tr').length + 1;

        tabindex = countd * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;

        if (AtTypeId != null && AtTypeId != "") {
            Option1 = "<option value='" + AtTypeId + "'>" + AtType + "</option>";
        }
        else {

            if (Holiday == true) {
                Option1 = "<option value='5' selected='selected'>HOLIDAY</option>";
            } else {
                Option1 = "<option value='4' selected='selected'>PRESENT</option>";
            }
        }
        var required = "";
        if (countd == 1) {
            required = 'required="required"';
        }
        if (action != '') {
            type = action;
        }
        var htype = "";
        var hdis = "";
        if (Holiday == true) {
            hdis = "disabled='disabled'";
            htype = "readonly = 'readonly'";
        }

        OverTime = OverTime != null ? OverTime : 0;
        EmpName = EmpName != null ? EmpName :"";

        data = "<td class='text-center'><input type='text' id='empname_" + countd + "' value='" + EmpName + "'  class='empname_" + countd + " form-control text-center'  tabindex='" + tab1 + "' readonly='readonly'/></td>" +
               "<td class='input-group input-group-sm' width='100%'><select  name='addattendanced[].AtType' data-name='AttendanceType' class='form-control AtType' " + required + " data-id='" + countd + "' tabindex='" + tab2 + "' " + htype + " onchange='GetAttypechange(this," + countd + ")' id='attenedencetype_" + countd + "' " + hdis + ">" + Option1 + "</select></td>" +
               "<td><input type='number' name='addattendanced[].OverTime' id='overtime_" + countd + "' value='" + OverTime + "'  class='overtime_" + countd + " form-control text-right OverTime' tabindex='" + tab3 + "' onchange='overtime_change();'/>" +
               "<input type='hidden' name='addattendanced[].EmployeeId' id='empid_" + countd + "' value='" + EmpId + "'  class='empid_" + countd + " form-control text-center EmpId' /> ";
        "</td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        resetDeptStrBtn();
        searchAtType();
        countd++;
        setTabIndex();
    }

    function resetDeptStrBtn() {
        var i = 0;
        $('.dailyat').each(function (index, element) {
            //var input1 = $(this).find('.seldate')
            //input1.attr('name', 'addattendance[' + i + '].AtDate');

            var input2 = $(this).find('.AtType');
            input2.attr('name', 'addattendanced[' + i + '].AtType');

            var input3 = $(this).find('.OverTime');
            input3.attr('name', 'addattendanced[' + i + '].OverTime');

            var input4 = $(this).find('.EmpId');
            input4.attr('name', 'addattendanced[' + i + '].EmployeeId');
            i++;
        });

    }
}

