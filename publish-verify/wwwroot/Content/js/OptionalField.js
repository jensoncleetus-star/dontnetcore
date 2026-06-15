var count = 1, type = '';
limits = 500;
function addopfield(t, action, Name, active, print, id, Type) {
    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var Option1 = "";
        var Option2 = "";
        var required = "";
        var divid = "opfield_" + Name;
        var data = "";
        var a = "item_name" + count,
        tabindex = count * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        var slno = $('#addoption tr').length + 1;
        var row = "<tr class='opfield_" + Name + "' id='opfield_" + count + "'>";
        if (count == 1) {
            required = 'required="required"';
        }
        var checkactive = "";
        var checkprint = "";
        if (active == true) {
            checkactive = "checked";
        }
        if (print == true) {
            checkprint = "checked";
        }
        var Section = $('#Section').val();
        
        if (Section == "Task" || Section == "Sales" || Section == "SReturn" || Section == "LPO" || Section == "Purchase" || Section == "PReturn" || Section == "Quot") {
            if (Type == "Text") {
                Option1 = "<option value='Text' selected='selected'>Text</option>";
            } else {
                Option1 = "<option value='Text'>Text</option>";
            }
            if (Type == "Dropdown") {
                Option2 = "<option value='Dropdown' selected='selected'>Dropdown</option>";
            } else {
                Option2 = "<option value='Dropdown'>Dropdown</option>";
            }
        }

        if (Section == "Leads") {          
            if (Type == "Text") {
                Option1 = "<option value='Text' selected='selected'>Text</option>";
            } else {
                Option1 = "<option value='Text'>Text</option>";
            }
            if (Type == "Dropdown") {
                Option2 = "<option value='Dropdown' selected='selected'>Dropdown</option>";
            } else {
                Option2 = "<option value='Dropdown'>Dropdown</option>";
            }
        }

        if (Section == "AMC") {
            if (Type == "Text") {
                Option1 = "<option value='Text' selected='selected'>Text</option>";
            } else {
                Option1 = "<option value='Text'>Text</option>";
            }
            if (Type == "Dropdown") {
                Option2 = "<option value='Dropdown' selected='selected'>Dropdown</option>";
            } else {
                Option2 = "<option value='Dropdown'>Dropdown</option>";
            }
        }
        if (Section == "MR") {
            if (Type == "Text") {
                Option1 = "<option value='Text' selected='selected'>Text</option>";
            } else {
                Option1 = "<option value='Text'>Text</option>";
            }
            if (Type == "Dropdown") {
                Option2 = "<option value='Dropdown' selected='selected'>Dropdown</option>";
            } else {
                Option2 = "<option value='Dropdown'>Dropdown</option>";
            }
        }
       
        var addprint = "";
        data = "<td style='width:50px;' class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td style='width:950px;'><input  name='listname' " + required + " tabindex='" + tab1 + "' value='" + Name + "' id='listname_" + count + "'  value='' class='listname_" + count + " form-control text-left listname' placeholder='Enter Field name' min='0''/></td> " +
                "<td style='width:50px;'><input  id='active_" + count + "' class='active_" + count + "' " + checkactive + " onchange='activechange(this," + count + ")' tabindex='" + tab2 + "' type=\"checkbox\" name=\"active\" ></input></td>" +
                "<td style='width:200px;'><select name='Type' class='form-control AtType' id='type_" + count + "'>" + Option1 + Option2 + "</select></td>" +
                "<input type='hidden' class='id_" + count + "' value='" + id + "'  name='id' id='id_" + count + "'/> " +
                "</td>";

        if ((Section != 'Project') && (Section != 'Task')) {
            addprint += "<td style='width:50px;'><input  id='print_" + count + "' class='print_" + count + "' " + checkprint + " onchange='printchange(this," + count + ")' tabindex='" + tab3 + "' type=\"checkbox\" name=\"print\" ></input></td>"
        }


        //"<td style='width:50px;' class='text-center'><button tabindex='" + tab3 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";

        row += data + addprint + "</tr>";
        $('#' + t).append(row); //alert($('#active_' + count).val());

        if (active == true) {
            $('#active_' + count).val("true");
        } else {
            $('#active_' + count).val("false");
        }
        if (print == true) {
            $('#print_' + count).val("true");
        } else {
            $('#print_' + count).val("false");
        }
        count++;
    }
}


function activechange(object, arg) {
    var active = ($('#active_' + arg).prop('checked'));
    var name = $('#listname_' + arg).val();

    if (active == true && name == '') {
        alert('Field Name is required');
        $("#active_" + arg).prop("checked", false);
    }
    else {
        var newval = (active == true) ? $('#active_' + arg).val("true") : $('#active_' + arg).val("false");
    }
}
function printchange(object, arg) {
    var print = ($('#print_' + arg).prop('checked'));
    var newval = (print == true) ? $('#print_' + arg).val("true") : $('#print_' + arg).val("false");
}

//Delete a row of table
function deleteRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'opfield_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    var i = 1;
    $('#addoption tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}