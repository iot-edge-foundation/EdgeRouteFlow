# EdgeRouteFlow

The repository shows the code behind http://iotedgemoduleflow.azurewebsites.net

## Flow diagram

Azure IoT Edge module routes are hard to read. Why not show them in a diagram?

![Flow diagram example](/images/fd.png)

## Blog Post

This website is based on the [blog post](https://sandervandevelde.wordpress.com/2019/01/25/visualize-azure-iot-edge-device-routes-as-a-flowchart-in-asp-net-mvc/) of [sandervandevelde](https://github.com/sandervandevelde). 

## Disclaimer

This site does not save input data, it does not track users. There are no ads. Just enjoy Azure IoT Edge and the flow charts of your routes.

We activated HTTPS to project your connectionstrings.

If you do not want to expose your production connection string outside Azure, you can use the option to submit only the Routes JSON.

*Update*: the new user interface of the Azure portal is not exposing the JSON of the routes anymore. You can still find it in the desired properties of the edgeHub module (in the manifest). 
