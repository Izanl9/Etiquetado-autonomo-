let modeloIA;

window.cargarModelo = async (url) => {
    const modelURL = url + "model.json";
    const metadataURL = url + "metadata.json";
    modeloIA = await tmImage.load(modelURL, metadataURL);
    console.log("¡Cerebro de IA cargado y listo en el móvil!");
};

window.predecirImagen = async (fotoBase64) => {
    return new Promise((resolve) => {
        const img = new Image();
        img.src = fotoBase64;
        img.onload = async () => {
            const predicciones = await modeloIA.predict(img);
            predicciones.sort((a, b) => b.probability - a.probability);
            resolve(predicciones[0].className); 
        };
    });
};