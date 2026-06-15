var count = 1, type = '';
limits = 500;
function addrow(t, action, Name, Note) {

    if (count == limits) alert("You have reached the limit of adding " + count + " inputs");
    else {
        var Option = "";
        var required = "";
        var divid = "checklist_" + Name;
        var data = "";
        var a = "item_name" + count,
        tabindex = count * 4;
        tab1 = tabindex + 1;
        tab2 = tabindex + 2;
        tab3 = tabindex + 3;
        var slno = $('#addchecklist tr').length + 1;
        var row = "<tr class='checklist_" + Name + "' id='checklist_" + count + "'>";
        if (count == 1) {
            required = 'required="required"';
        }
        var check = "";
        if (Note == true) {
            check = "checked";
        }
        data = "<td style='width:50px;' class='text-center' id=" + divid + "> " + slno + " </td>" +
                "<td style='width:550px;'><input  name='listname' " + required + " tabindex='" + tab1 + "' data-msg-required='The Item Rate field is required' value='" + Name + "' id='listname_" + count + "' onchange='namechange(this," + count + ")' value='' class='listname_" + count + " form-control text-left listname' placeholder='Enter List name' min='0''/></td> " +
                "<td style='width:50px;'><input  id='note_" + count + "' class='note_" + count + "' " + check + " tabindex='" + tab2 + "' onchange='notechange(this," + count + ")' type=\"checkbox\" name=\"note\" ></input></td>" +
                "<td style='width:50px;' class='text-center'><button tabindex='" + tab3 + "' style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='deleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>";
                "</td>";
                row += data + "</tr>";
                $('#' + t).append(row); //alert($('#note_' + count).val());

                if (Note == true) {
                    $('#note_' + count).val("true");
                } else {
                    $('#note_' + count).val("false");
                }

                count++;
    }
}

function namechange(object, arg) {

    if ($('#addchecklist tr').length == arg) {
        $(object).closest('tr').attr('class', "checklist_" + arg);
        addrow('addchecklist', 'checklist', '', '');
    }
}
function notechange(object, arg) {
    var $input = $('#note_' + arg).prop('checked');
    var newval = ($('#note_' + arg).prop('checked') == true) ? $('#note_' + arg).val("true") : $('#note_' + arg).val("false");
    //alert($('#note_' + arg).val());
}

//Delete a row of table
function deleteRow(t) {
    var classname = $(t).closest('tr').attr('class');
    if (classname == 'checklist_') alert("Sorry You Can't Delete This Row.");
    else {
        var r = confirm("Are you sure you want to delete this..?");
        if (r == true) {
            var e = t.parentNode.parentNode;
            e.parentNode.removeChild(e);
        }
    }
    var i = 1;
    $('#addchecklist tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}