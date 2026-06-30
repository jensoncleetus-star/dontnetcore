
//Add Row function
function AssetToInventoryAddRow(count, t, AssetID, AssetName, EntryID, Barcode, UnitID, UnitName, Quantity, Price, Amount, RefItemID, DeprPerc, DeprAccountID, DeprAccountName, McToID) {

    var slno    =   $('#addinvoiceItem tr').length + 1;
    var row     =   "<tr class='item_' id='item_" + count + "'>";
    var data    =   "";
    var divid   =   "item_name_" + AssetID;

    if (AssetID != '') {
        row = "<tr class='item_" + AssetID + "' id='item_" + count + "'>";
    }

    data =  "<td class='text-center' id=" + divid + "> " + slno + " </td>" +
            "<td class='hide'><input  name='AssetID'            id='AssetID_" + count + "'      value='" + AssetID + "'/></td>" +
            "<td class='hide'><input  name='UnitID'             id='UnitID_" + count + "'       value='" + UnitID + "'/></td>" +
            "<td class='hide'><input  name='EntryID'            id='EntryID_" + count + "'      value='" + EntryID + "'/></td>" +
            "<td class='hide'><input  name='RefItemID'          id='RefItemID_" + count + "'    value='" + RefItemID + "'/></td>" +
            "<td class='hide'><input  name='DeprAccountID'      id='DeprAccountID_" + count + "'value='" + DeprAccountID + "'/></td>" +
            "<td ><input type='text'  name='AssetName'          id='ItemName_" + count + "'     value='" + AssetName + "'      class='form-control text-center' readonly='readonly' style='width:300px;'/></td>" +
            "<td><input type='text'   name='Barcode'            id='Barcode_" + count + "'      value='" + Barcode + "'        class='form-control text-center' readonly='readonly'/> </td>" +
            "<td><input type='text'   name='Unit'               id='Unit_" + count + "'         value='" + UnitName + "'       class='form-control text-center' readonly='readonly'/></td>" +
            "<td><input type='text'   name='DeprPerc'           id='DeprPerc_" + count + "'     value='" + DeprPerc + "'       class='form-control text-center' readonly='readonly'/></td>" +
            "<td><input type='text'   name='DeprAccName'        id='DeprAccName" + count + "'   value='" + DeprAccountName + "'class='form-control text-center' readonly='readonly' style='width:170px;' /></td>" +
            "<td><input type='number' name='product_quantity[]' id='total_qntt_" + count + "'   value='" + Quantity + "'       class='total_qntt_" + count + " form-control text-right qty' onchange='quantity_change(" + count + ',' + Quantity + ");'/></td>" +
            "<td><input type='number' name='product_rate[]'     id='price_item_" + count + "'   value='" + Price + "'          class='price_item_" + count + " form-control text-right totrate'  readonly='readonly'/></td>" +
            "<td><input type='number' name='sub_total[]'        id='sub_total_" + count + "'    value='" + Amount + "'         class='sub_total_" + count + " form-control text-right subtotal'  readonly='readonly'/></td>" +
            "<td class='text-center'><button  style='text-align: right;' class='btn btn-danger' type='button' value='Delete' onclick='AssetToInventorydeleteRow(this)'><i class='fa fa-trash fa-1x'></i></button></td>" +
            "<td class='hide'><input  name='McToID'             id='McToID_" + count + "'       value='" + McToID + "'/></td>";
    var dltchkbox = "<td class='text-center'><input type='checkbox' name='dltcheck' value='' checked></td>";
    row =row+ data + dltchkbox+ "</tr>";
    $('#' + t).append(row);

    //Disable Column Quantity, if there is no data
    if (Quantity == "")
        $("#total_qntt_" + count).attr('readonly', true);

    count++;

    if (AssetID != '')
        AssetToInventoryCalculateSum();
    else {
        $("[id$=ToItemCount]").text("");
        $("[id$=ToItemQnty]").text("");
        $("[id$=ToItemPrice]").text("");
        $("[id$=ToItemAmount]").text("");
    }
}
function deleteAll() {
    var cnt = 0;
    $("input[name='dltcheck']:checked").each(function () {
        cnt++;
    });
    if (cnt != 0) {
        var r = confirm("Are you sure you want to delete this..?");
    }

    $("input[name='dltcheck']:checked").each(function () {

        var classname = $("input[name='dltcheck']:checked").closest('tr').attr('class');


        if (classname == 'item_') {
            $(this).prop('checked', false);
        }
        else {

            if (r == true) {
                var trid = $("input[name='dltcheck']:checked").closest('tr').attr('id');

                var fields = trid.split('_');
                var dataid = fields[1];
                var e = this.parentNode.parentNode;

                e.parentNode.removeChild(e);
                // delete batch model
                $("#batch-" + dataid).remove();
            }
        }
    });
    AssetToInventoryCalculateSum();
}
function quantity_change(arg, AssetQty) {

    var CurrentQty = $("#total_qntt_" + arg).val();

    if (CurrentQty > AssetQty) {

        alert("Qty should be less than or equal to Total Asset Quantity : " + AssetQty)

        $("#total_qntt_" + arg).val(AssetQty)

        CurrentQty = AssetQty;
    }

    var UnitPrice = $("#price_item_" + arg).val();

    $("#sub_total_" + arg).val((UnitPrice * CurrentQty).toFixed(2));

    //Calling function to set the text in footer(Total)
    AssetToInventoryCalculateSum();
}

//Function to set the text in footer(Total)
function AssetToInventoryCalculateSum() {

    var tbody = $("#AssetDetails tbody");
    if (tbody.children().length > 0) {

        var gtQty = 0;
        var gtSubTotal = 0;
        var gtRate = 0;

        $(".qty").each(function () {
            var subQty = this.value;
            gtQty = parseFloat(gtQty) + parseFloat(subQty);
        });

        $(".totrate").each(function () {
            var ttr = $(this).val();
            ttr = ttr || 0;
            gtRate = parseFloat(gtRate) + parseFloat(ttr);
        });

        $(".subtotal").each(function () {
            var subTot = this.value;
            gtSubTotal = parseFloat(gtSubTotal) + parseFloat(subTot);
        });

        $("[id$=ToItemCount]").text(tbody.children().length);
        $("[id$=ToItemQnty]").text((gtQty));
        $("[id$=ToItemPrice]").text((gtRate).toFixed(2));
        $("[id$=ToItemAmount]").text((gtSubTotal).toFixed(2));

        //Setting the total value to a textbox (for taking the value for saving)
        $("#GrandTotal").val(parseFloat(gtSubTotal).toFixed(2));
    }
}

//Delete a row of table
function AssetToInventorydeleteRow(t) {

    var classname = $(t).closest('tr').attr('class');

    //If there is no data, can't delete the row
    if (classname == 'item_')
        alert("Sorry you can't delete this row.");
    else {

        var RowCount = $('#addinvoiceItem tr').length

        //If there is only one row, can't delete that row
        if (RowCount == 1)
            alert("Sorry you can't delete this row.");
        else {
            var r = confirm("Are you sure you want to delete this..?");

            if (r == true) {
                var e = t.parentNode.parentNode;
                e.parentNode.removeChild(e);

                //Calling function to set the text in footer(Total)
                AssetToInventoryCalculateSum();
            }
        }
    }

    var i = 1;

    $('#addinvoiceItem tr').each(function () {
        $(this).find('td:first').text(i);
        i++;
    });
}