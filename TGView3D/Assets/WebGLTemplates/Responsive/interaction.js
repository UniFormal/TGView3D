$("document").ready(function(){

    $("#file").change(function(e) {
		var reader = new FileReader();
		reader.onload = function(event)
        {
			var son = event.target.result;
			gameInstance.SendMessage("Main", "WebBrowserLoad", son);
			
        };
		reader.readAsText(e.target.files[0]);
 
      
    });
});