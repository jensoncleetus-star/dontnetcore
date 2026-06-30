var count = 1, type = '';
limits = 500;
function addempitem(t, action, PayHead, PayHeadId, Rate,Per, HeadType, CalType, Computed,Item) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var data = "";
        var Type = "";
        var Option1 = "";
        var readonly = "";
        var row = "";
        if (PayHeadId != null) {
            row = "<tr class='payhead emp_" + PayHeadId + "' id='emp_" + count + "'>";
        } else {
            row = "<tr class='payhead emp_' id='emp_" + count + "'>";
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
            Option1 = "<option value='" + PayHeadId + "'>" + PayHead + "</option>";
        }
        var required = "";
        var readonly = "";
        if (count == 1) {
            required = 'required="required"';
        }
        if (CalType == "DefinedValue" || CalType == "AsComputedValue") {
            readonly = 'readonly="readonly"';
        }

        if (action != '') {
            type = action;
        }
        var attaddbtn = "<span class='input-group-btn'><a type='button' href='/Hr/Payhead/AddPayhead' class='modal-create btn btn-success btn-flat'><i class='fa fa-1x fa-plus-circle'></i></a></span>";

        Rate = Rate != null ? Rate : 0;
        Per = Per != null ? Per : "";
        HeadType = HeadType != null ? HeadType : "";
        CalType = CalType != null ? CalType : "";
        //alert(CalType);
        CalType = (CalType == "DefinedValue") ? "As User Defined Value" : (CalType == "AsComputedValue") ? "As Computed Value" : (CalType == "FlatRate") ? "Flat Rate" : (CalType == "OnAttendance") ? "On Attendance" : "On Production";
        Computed = Computed != null ? Computed : "";
        data = "<td class='text-center'> " + slno + " </td>" +
               "<td class='input-group input-group-sm' width='250px'><select name='salarystr[].PayHeadId' class='form-control ddlPayHead' " + required + " data-id='" + count + "' placeholder='Pay Head Name' id='payheadId_" + count + "' data-msg-required='Pay Head is required' onchange='GetPHeadDetails(this," + count + ",\"" + type + "\")'>" + Option1 + "</select>" + attaddbtn + "</td>" +
               "<td><input type='number' name='salarystr[].Rate' id='value_" + count + "' " + readonly + " value='" + Rate + "'  class='value_" + count + " form-control text-right Rate' placeholder='0' value='0' tabindex='" + tab2 + "'/></td>" +
               "<td><input type='text' id='per_" + count + "' value='" + Per + "'  class='per_" + count + " form-control text-center'  tabindex='" + tab3 + "' readonly='readonly'/></td>" +
               "<td><input type='text' id='headtype_" + count + "' value='" + HeadType + "'  class='headtype_" + count + " form-control text-center'  tabindex='" + tab4 + "' readonly='readonly'/></td>" +
               "<td><input type='text' id='caltype_" + count + "' value='" + CalType + "'  class='caltype_" + count + " form-control text-center'  tabindex='" + tab5 + "' readonly='readonly'/></td>" +
               "<td><input type='text' id='compute_" + count + "' value='" + Computed + "'  class='compute_" + count + " form-control text-center'  tabindex='" + tab6 + "' readonly='readonly'/></td>" +
               "<td class='text-center'><button style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";
        row += data + "</tr>";

        $('#' + t).append(row);
        searchPayHead();
        resetStrBtn();

        count++;
        setTabIndex();
    }
}
function resetStrBtn() {
    var i = 0;
    $('.payhead').each(function (index, element) {
        var input1 = $(this).find('.ddlPayHead')
        input1.attr('name', 'salarystr[' + i + '].PayHeadId');

        var input2 = $(this).find('.Rate');
        input2.attr('name', 'salarystr[' + i + '].Rate');

        i++;
    });

}

function searchPayHead() {
    var selecteditem = new Array();
    $(".ddlPayHead").each(function () {
        selecteditem.push($(this).val());
    });

    $(".ddlPayHead").select2({
        placeholder: 'Search by Name ',
        minimumInputLength: 0,
        ajax: {
            url: "/Hr/Payhead/SearchPayheadForSalary",
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
                        more: false
                    }
                };
            },
            cache: true
        },
    });
}
//item details
function GetPHeadDetails(selectObject, dataid, action) {
    if (selectObject.value) {
        var EmpId = selectObject.value;
        if (EmpId != null) {
            if ($(".emp_" + EmpId).length > 0) {
                alert("Sorry You Cant Add An Payhead More Than One Time..!!");
                $(selectObject).val(null).trigger('change');
            }
            else {
                $(selectObject).closest('tr').attr('class', "payhead emp_" + EmpId);
                if ($(".emp_").length == 0) {
                    addempitem('addempitem', "", "", "", "0.00", "", "");
                }
                itemUpdate(selectObject, dataid, action);
                resetStrBtn();
            }
        }
    }
}
function itemUpdate(selectObject, dataid, action) {
    $.ajax({
        url: '/Payhead/GetPayheadById',
        type: "GET",
        dataType: "JSON",
        data: { pyID: selectObject.value },
        success: function (result) {
            $(".per_" + dataid).val(result.Per);
            $(".headtype_" + dataid).val(result.HeadType);
            var CalType = (CalType == "DefinedValue") ? "As User Defined Value" : (CalType == "AsComputedValue") ? "As Computed Value" : (CalType == "FlatRate") ? "Flat Rate" : (CalType == "OnAttendance") ? "On Attendance" : "On Production";

            $(".caltype_" + dataid).val(CalType);
            $(".compute_" + dataid).val(result.Computed);
            if (result.CalType == "DefinedValue" || result.CalType == "AsComputedValue") {
                $('#value_' + dataid).prop('readonly', true);
            } else {
                $('#value_' + dataid).prop('readonly', false);
            }
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
    if (classname == 'payhead emp_') alert("Sorry You Can't Delete This Row.");
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