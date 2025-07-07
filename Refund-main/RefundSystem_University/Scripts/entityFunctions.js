function editEntity(id, controllerName, formModalId) {
    toggleLoader(true);
    $.ajax({
        url: '/' + controllerName + '/_Edit',
        method: 'POST',
        data: { id: id },
        success: function (result) {
            $('#' + formModalId + '-form-modal .modal-body').html(result);
            $('#' + formModalId + '-form-modal').modal('show');
        },
        error: function () {
            alert('לא ניתן לטעון את הנתונים')
        }
    }).always(function () {
        toggleLoader(false);
    });
}

function saveEntity(controllerName, formModalId) {
    if (!$("#" + formModalId + "-form-modal form").valid())
        return false;
    toggleLoader(true);
    let formData = new FormData(document.querySelector("#" + formModalId + "-form-modal form"));
    $.ajax({
        url: '/' + controllerName + '/Save',
        method: 'POST',
        data: formData, //$("#" + formModalId + "-form-modal form").serialize(),
        processData: false,
        contentType: false,
        success: function (result) {
            if (result.Success) {
                $("#" + formModalId + "-form-modal form #Id").remove()
                $("#" + formModalId + "-form-modal form input:not(.preserve-value),#" + formModalId + "-form-modal form select").val(null);
                $("#" + formModalId + "-form-modal").modal('hide');
                location.reload();
            }
            else {
                alert(result.Message);
                console.log(result.Exception);
            }
        },
        error: function () {
            alert('טעינת הנתונים נכשלה');
        }
    }).always(toggleLoader(false));
}

function deleteEntity(e, id, controllerName) {
    if (confirm('האם אתה בטוח שברצונך למחוק את הרשומה?')) {
        toggleLoader(true);
        $.ajax({
            url: '/' + controllerName + '/Delete',
            method: 'POST',
            data: { id: id, "__RequestVerificationToken": $('input[name=__RequestVerificationToken]').val() },
            success: function (result) {
                debugger;
                if (result.Success)
                    $(e).parents('tr').remove();
                else {
                    alert(result.Message)
                    console.log(result.Exception)
                }
            },
            error: function (xhr) {
                alert('המחיקה נכשלה');
            }
        }).always(toggleLoader(false));
    }
    return false;
}