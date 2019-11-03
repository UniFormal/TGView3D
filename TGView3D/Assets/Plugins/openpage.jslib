 mergeInto(LibraryManager.library, {
		
         openWindow: function (url) {
				
             window.open(Pointer_stringify(url));
         },

          download: function(rdata, rstrFileName, rstrMimeType) {

			data = Pointer_stringify(rdata);
			strFileName = Pointer_stringify(rstrFileName);
			strMimeType = Pointer_stringify(rstrMimeType);

			var self = window, // this script is only for browsers anyway...
			defaultMime = "application/octet-stream", // this default mime also triggers iframe downloads
			mimeType = strMimeType || defaultMime,
			payload = data,
			url = !strFileName && !strMimeType && payload,
			anchor = document.createElement("a"),
			toString = function(a){return String(a);},
			myBlob = (self.Blob || self.MozBlob || self.WebKitBlob || toString),
			fileName = strFileName || "download",
			blob,
			reader;
			myBlob= myBlob.call ? myBlob.bind(self) : Blob ;
	  
		if(String(this)==="true"){ //reverse arguments, allowing download.bind(true, "text/xml", "export.xml") to act as a callback
			payload=[payload, mimeType];
			mimeType=payload[0];
			payload=payload[1];
		}


		if(url && url.length< 2048){ // if no filename and no mime, assume a url was passed as the only argument
			fileName = url.split("/").pop().split("?")[0];
			anchor.href = url; // assign href prop to temp anchor
		  	if(anchor.href.indexOf(url) !== -1){ // if the browser determines that it's a potentially valid url path:
        		var ajax=new XMLHttpRequest();
        		ajax.open( "GET", url, true);
        		ajax.responseType = 'blob';
        		ajax.onload= function(e){ 
				  download(e.target.response, fileName, defaultMime);
				};
        		setTimeout(function(){ ajax.send();}, 0); // allows setting custom ajax headers using the return:
			    return ajax;
			} // end if valid url?
		} // end if url?


		//go ahead and download dataURLs right away
		if(/^data\:[\w+\-]+\/[\w+\-]+[,;]/.test(payload)){
		
			if(payload.length > (1024*1024*1.999) && myBlob !== toString ){

				var parts= payload.split(/[:;,]/),
				type= parts[1],
				decoder= parts[2] == "base64" ? atob : decodeURIComponent,
				binData= decoder( parts.pop() ),
				mx= binData.length,
				i= 0,
				uiArr= new Uint8Array(mx);
	
				for(i;i<mx;++i) uiArr[i]= binData.charCodeAt(i);
	
			


				payload=new myBlob([uiArr], {type: type});
				mimeType=payload.type || defaultMime;
			}else{	
				
				var parts= payload.split(/[:;,]/),
				type= parts[1],
				decoder= parts[2] == "base64" ? atob : decodeURIComponent,
				binData= decoder( parts.pop() ),
				mx= binData.length,
				i= 0,
				uiArr= new Uint8Array(mx);
	
				for(i;i<mx;++i) uiArr[i]= binData.charCodeAt(i);

				
				if( navigator.msSaveBlob )  // IE10 can't do a[download], only Blobs:
					return navigator.msSaveBlob(dataUrlToBlob(payload), fileName);
				else{
					//	saver(payload) ; // everyone else can save dataURLs un-processed
					var url = payload;
					var winMode = false;
					if ('download' in anchor) { //html5 A[download]
						anchor.href = url;
						anchor.setAttribute("download", fileName);
						anchor.className = "download-js-link";
						anchor.innerHTML = "downloading...";
						anchor.style.display = "none";
						document.body.appendChild(anchor);
						setTimeout(function() {
							anchor.click();
							document.body.removeChild(anchor);
							if(winMode===true){setTimeout(function(){ self.URL.revokeObjectURL(anchor.href);}, 250 );}
						}, 66);
						return true;
					}

					// handle non-a[download] safari as best we can:
					if(/(Version)\/(\d+)\.(\d+)(?:\.(\d+))?.*Safari\//.test(navigator.userAgent)) {
						url=url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
						if(!window.open(url)){ // popup blocked, offer direct download:
							if(confirm("Displaying New Document\n\nUse Save As... to download, then click back to return to this page.")){ location.href=url; }
						}
						return true;
					}

					//do iframe dataURL download (old ch+FF):
					var f = document.createElement("iframe");
					document.body.appendChild(f);

					if(!winMode){ // force a mime that will download:
						url="data:"+url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
					}
					f.src=url;
					setTimeout(function(){ document.body.removeChild(f); }, 333);
				}
			}
			
		}//end if dataURL passed?

		blob = payload instanceof myBlob ?
			payload :
			new myBlob([payload], {type: mimeType}) ;








		if (navigator.msSaveBlob) { // IE10+ : (has Blob, but not a[download] or URL)
			return navigator.msSaveBlob(blob, fileName);
		}

		if(self.URL){ // simple fast and modern way using Blob and URL:
			//saver(self.URL.createObjectURL(blob), true);
			var url = self.URL.createObjectURL(blob);
			var winMode = true;
			if ('download' in anchor) { //html5 A[download]
				anchor.href = url;
				anchor.setAttribute("download", fileName);
				anchor.className = "download-js-link";
				anchor.innerHTML = "downloading...";
				anchor.style.display = "none";
				document.body.appendChild(anchor);
				setTimeout(function() {
					anchor.click();
					document.body.removeChild(anchor);
					if(winMode===true){setTimeout(function(){ self.URL.revokeObjectURL(anchor.href);}, 250 );}
				}, 66);
				return true;
			}

			// handle non-a[download] safari as best we can:
			if(/(Version)\/(\d+)\.(\d+)(?:\.(\d+))?.*Safari\//.test(navigator.userAgent)) {
				url=url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
				if(!window.open(url)){ // popup blocked, offer direct download:
					if(confirm("Displaying New Document\n\nUse Save As... to download, then click back to return to this page.")){ location.href=url; }
				}
				return true;
			}

			//do iframe dataURL download (old ch+FF):
			var f = document.createElement("iframe");
			document.body.appendChild(f);

			if(!winMode){ // force a mime that will download:
				url="data:"+url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
			}
			f.src=url;
			setTimeout(function(){ document.body.removeChild(f); }, 333);

	
		}else{
			// handle non-Blob()+non-URL browsers:
			if(typeof blob === "string" || blob.constructor===toString ){
				try{
					//return saver( "data:" +  mimeType   + ";base64,"  +  self.btoa(blob)  );
					var url ="data:" +  mimeType   + ";base64,"  +  self.btoa(blob) ;
					var winMode = false;
					if ('download' in anchor) { //html5 A[download]
						anchor.href = url;
						anchor.setAttribute("download", fileName);
						anchor.className = "download-js-link";
						anchor.innerHTML = "downloading...";
						anchor.style.display = "none";
						document.body.appendChild(anchor);
						setTimeout(function() {
							anchor.click();
							document.body.removeChild(anchor);
							if(winMode===true){setTimeout(function(){ self.URL.revokeObjectURL(anchor.href);}, 250 );}
						}, 66);
						return true;
					}
		
					// handle non-a[download] safari as best we can:
					if(/(Version)\/(\d+)\.(\d+)(?:\.(\d+))?.*Safari\//.test(navigator.userAgent)) {
						url=url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
						if(!window.open(url)){ // popup blocked, offer direct download:
							if(confirm("Displaying New Document\n\nUse Save As... to download, then click back to return to this page.")){ location.href=url; }
						}
						return true;
					}
		
					//do iframe dataURL download (old ch+FF):
					var f = document.createElement("iframe");
					document.body.appendChild(f);
		
					if(!winMode){ // force a mime that will download:
						url="data:"+url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
					}
					f.src=url;
					setTimeout(function(){ document.body.removeChild(f); }, 333);

					
				}catch(y){
					//return saver( "data:" +  mimeType   + "," + encodeURIComponent(blob)  );
					var url = "data:" +  mimeType   + "," + encodeURIComponent(blob)  ;
					var winMode = false;
					if ('download' in anchor) { //html5 A[download]
						anchor.href = url;
						anchor.setAttribute("download", fileName);
						anchor.className = "download-js-link";
						anchor.innerHTML = "downloading...";
						anchor.style.display = "none";
						document.body.appendChild(anchor);
						setTimeout(function() {
							anchor.click();
							document.body.removeChild(anchor);
							if(winMode===true){setTimeout(function(){ self.URL.revokeObjectURL(anchor.href);}, 250 );}
						}, 66);
						return true;
					}
		
					// handle non-a[download] safari as best we can:
					if(/(Version)\/(\d+)\.(\d+)(?:\.(\d+))?.*Safari\//.test(navigator.userAgent)) {
						url=url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
						if(!window.open(url)){ // popup blocked, offer direct download:
							if(confirm("Displaying New Document\n\nUse Save As... to download, then click back to return to this page.")){ location.href=url; }
						}
						return true;
					}
		
					//do iframe dataURL download (old ch+FF):
					var f = document.createElement("iframe");
					document.body.appendChild(f);
		
					if(!winMode){ // force a mime that will download:
						url="data:"+url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
					}
					f.src=url;
					setTimeout(function(){ document.body.removeChild(f); }, 333);
				}
			}

			// Blob but not URL support:
			reader=new FileReader();
			reader.onload=function(e){
				//saver(this.result);
				var url = this.result;
				var winMode = false;
				if ('download' in anchor) { //html5 A[download]
					anchor.href = url;
					anchor.setAttribute("download", fileName);
					anchor.className = "download-js-link";
					anchor.innerHTML = "downloading...";
					anchor.style.display = "none";
					document.body.appendChild(anchor);
					setTimeout(function() {
						anchor.click();
						document.body.removeChild(anchor);
						if(winMode===true){setTimeout(function(){ self.URL.revokeObjectURL(anchor.href);}, 250 );}
					}, 66);
					return true;
				}
	
				// handle non-a[download] safari as best we can:
				if(/(Version)\/(\d+)\.(\d+)(?:\.(\d+))?.*Safari\//.test(navigator.userAgent)) {
					url=url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
					if(!window.open(url)){ // popup blocked, offer direct download:
						if(confirm("Displaying New Document\n\nUse Save As... to download, then click back to return to this page.")){ location.href=url; }
					}
					return true;
				}
	
				//do iframe dataURL download (old ch+FF):
				var f = document.createElement("iframe");
				document.body.appendChild(f);
	
				if(!winMode){ // force a mime that will download:
					url="data:"+url.replace(/^data:([\w\/\-\+]+)/, defaultMime);
				}
				f.src=url;
				setTimeout(function(){ document.body.removeChild(f); }, 333);



			};
			reader.readAsDataURL(blob);
		}
		return true;


		}
 
 });
 