let photo = document.getElementById('imgPhoto');
let photoTela = document.getElementById('imgId');
let file = document.getElementById('filImage');

photo.addEventListener('click', () => {
    file.click();
});

file.addEventListener('change', (e) => {
    let reader = new FileReader();

    reader.onload = () => {
        photo.src = reader.result;
    }
    fileUpload();
});

function fileUpload() {

    var fileupload = $("#filImage").get(0);
    var files = fileupload.files;

    var fileData = new FormData();

    for (var i = 0; i < files.length; i++) {
        fileData.append(files[i].name, files[i]);
    }

    $.ajax({
        url: '/Account/UploadFoto',
        type: 'POST',
        contentType: false,
        processData: false,
        data: fileData,
        success: function (result) {
            console.log(result);
            var imagem = result.img;
            photo = imagem;
            window.location.reload(true);
        },
        error: function (error) {
            console.log(error.responseText);
        }
    });
}
