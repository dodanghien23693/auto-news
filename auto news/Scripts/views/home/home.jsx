
function InitData(){
    $.ajax({
        url: 'http://autonews.dev/api/sources',
        dataType: 'json',
        cache: false,
        success: function(data) {
            window.NewsSources =  data;
        }.bind(this),
        error: function(xhr, status, err) {
            console.error(url, status, err.toString());
        }.bind(this)
    });
}

InitData();


function FindObject(arrayObject,attribute,value){
    if(arrayObject!=undefined){
        for(var i=0;i<arrayObject.length;i++){
            var o = arrayObject[i];
            if(o[attribute]==value) {
                return o;
            }
        }
    }
    
    return undefined;
}

var SourcesConfig = React.createClass({
    getInitialState: function () {
        return { sources: [], selectedSources :[]}
    },
    componentDidMount: function () {
        $.ajax({
            url: "/api/sources",
            dataType: 'json',
            cache: false,
            success: function (data) {
                this.setState({ sources: data, selectedSources: this.state.selectedSources });

                var options = {
                    data: data,

                    getValue: "name",

                    //template: {
                    //    type: "description",
                    //    fields: {
                    //        description: "name"
                    //    }
                    //},

                    list: {
                        match: {
                            enabled: false
                        },
                        onSelectItemEvent: function () {
                            var sourceId = $("#sources-config").getSelectedItemData().id;
                            this.newSourceId = sourceId;
                            $("#selected-source-id").val(sourceId);
                        }.bind(this)
                    },

                    theme: "plate-dark"
                };

                $("#sources-config").easyAutocomplete(options);
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });

        $.ajax({
            url: "/api/sources/selected",
            dataType: 'json',
            cache: false,
            success: function (data) {
                this.setState({ sources: this.state.sources, selectedSources: data });
                
            }.bind(this),
            error: function (xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });   

    },
    newSourceId:-1,
    handleAddSource:function(){
        //var newSourceId = $("#selected-source-id").val()
        var s = FindObject(this.state.selectedSources, 'id', this.newSourceId);
        if (s != undefined) {
            alert("Nguồn ngày đã tồn tại! vui lòng chọn nguồn khác");
        }
        else {
            
            //console.log(source);
            var source = FindObject(this.state.sources, 'id', this.newSourceId);
            if (source != undefined) {
                
                var newArray = this.state.selectedSources;
                newArray.push(source);
                this.setState({ sources: this.state.sources, selectedSources: newArray });
                $("#sources-config").val("");
                this.newSourceId = -1;

                $.ajax({
                    url: "/api/newsSourceConfig/Add/"+source.id,
                    type: "POST",
                    dataType: 'json',
                    cache: false,
                    success: function (data) {
                        
                    },
                    error: function (xhr, status, err) {
                        
                    }.bind(this)
                });
            }
        }
    },
    handleRemoveSource: function (e) {
        e.preventDefault();
        var sourceId = parseInt($(e.target).parent().attr("value"));

        var newArray = [];
        for (var i = 0; i < this.state.selectedSources.length; i++) {
            if (this.state.selectedSources[i].id != sourceId) newArray.push(this.state.selectedSources[i]);
        }
        this.setState({ sources: this.state.sources, selectedSources: newArray });

        $.ajax({
            url: "/api/newsSourceConfig/Remove/" + sourceId,
            type: "POST",
            dataType: 'json',
            cache: false,
            success: function (data) {
                
            }.bind(this),
            error: function (xhr, status, err) {
                //console.error(this.props.url, status, err.toString());
            }
        });
    },
    render: function () {
        return (
                <div style={{position:'relative'}}>
            
                   
                    <input style={{width:'100%'}} id="sources-config" />
                    <span onClick={this.handleAddSource} id="add-source-btn" className="btn glyphicon glyphicon-plus pull-right" style={{position:'absolute',right:'0'}} ></span>
                
                    <input type="hidden" id="selected-source-id" />

                    {this.state.selectedSources.map(function(source){
                        return <SourceItemConfig key={source.sourceId} data={source} handleRemoveSource={this.handleRemoveSource} />
                    }.bind(this))}

                </div>
            );
}
});


var SourceItemConfig = React.createClass({
    render: function () {
        return (
                <div>{this.props.data.name}<a href="#" value={this.props.data.id} onClick={this.props.handleRemoveSource}  className="pull-right" ><span className="glyphicon glyphicon-remove"></span></a></div>
            );
    }
});

var ListArticle = React.createClass({
    getInitialState: function() {
        return {articles: [],page:1};
    },
    componentDidMount: function() {
        $.ajax({
            url: this.props.url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                this.setState({articles: data,page:1});
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });


    },
    loadMore:function(){
        var nextPage = this.state.page +1;
        $.ajax({
            url: this.props.url+"&page="+nextPage,
            dataType: 'json',
            cache: false,
            success: function(data) {
                var newArticles = this.state.articles.concat(data);
                this.setState({articles: newArticles,page:nextPage});
                if(data[0]!=undefined){
                    var nextFirstArticleUrl = data[0].originUrl;

                    $("a[href='"+nextFirstArticleUrl+"']").get(0).closest(".well").scrollIntoView();
                }
                
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });
    },
    handleClick :function(e){
       
        e.preventDefault();
        var element = $(e.target);
        $(element).closest(".panel").find(".panel-body").slideToggle(500);
            
        $(element).toggleClass("glyphicon-minus");
        $(element).toggleClass("glyphicon-plus");
            
        //alert("click");
    },
    handleShowMore :function(e){
        this.loadMore();
    },
    render: function() {
        return (
            <div className="panel panel-default">
                <div className="panel-heading group-title">{this.props.title}<a href="#" className="toggle-btn"><span onClick={this.handleClick}  className="icon glyphicon glyphicon glyphicon-minus pull-right"></span></a></div>
                <div className="panel-body">
                    <div className="article-list"> 
                        {this.state.articles.map(function(article){
                            return <Article key={article.id} data={article} />
                            })}
                    </div>
                    <div className="text-center"><a href="#" className="" onClick={this.handleShowMore}>Show more</a></div>
                </div>
               
            </div>
                );
            }
        }
    );

var Article = React.createClass({
    render: function() {
        var d = new Date(Date.parse(this.props.data.createTime));
        var relativeTime = timeSince(d);
        var source = FindObject(window.NewsSources,"id",this.props.data.newsSourceId);
        var sourceName = "";
        if(source!=undefined){
            var sourceName = source["name"];
        }

        var doc = $.parseHTML(this.props.data.rawContent);
        var imageUrl = this.props.data.imageUrl;
        if(imageUrl==""){
            
        }

        var description = this.props.data.description;

        if(description=="") {
            
            for(var i=0;i<doc.length;i++){
                description += $(doc[i]).text();
                if(description.length>200){
                    description = description.substring(0,200);
                    break;
                }
            }
        }
        
        //var facebookShareLink = 
        return (
            <div className="well well-sm"> 
            <div >
                <a href={this.props.data.originUrl} target="_blank">
                    <img src={this.props.data.imageUrl} className="img-responsive article-feature-image pull-left" width="150px"/>
                </a>
                <div className="article-info clearfix" style={{marginLeft: 160}}>
                    <h3 style={{marginTop: 0,fontSize:18}}><a href={"/detailarticle/"+this.props.data.id} className="article-title" target="_blank">{this.props.data.title}</a></h3>
                    <div className="source-info" style={{fontSize:13}}>{sourceName} - {relativeTime+"    "}‎ 
                    
                     <a href={"https://facebook.com/sharer.php?u="+window.location.host+"/detailarticle/"+this.props.data.id} title="" target="_blank" class="btn"><img width="13px" title="Chia sẻ trên Facebook" src="/Content/images/facebook.png"/></a>
                    </div>
                    <div className="article-description" style={{marginTop: 10, fontSize: 14}}>{description}</div>
                </div>
            </div>
            </div>
        );
}
});


        function timeSince(timeStamp) {
            var now = new Date(),
                secondsPast = (now.getTime() - timeStamp.getTime() ) / 1000;
            if(secondsPast < 60){
                return parseInt(secondsPast) + ' giây trước';
            }
            if(secondsPast < 3600){
                return parseInt(secondsPast/60) + ' phút trước';
            }
            if(secondsPast <= 86400){
                return parseInt(secondsPast/3600) + ' giờ trước';
            }
            if(secondsPast > 86400){
                var day = timeStamp.getDate();
                var month = timeStamp.toDateString().match(/ [a-zA-Z]*/)[0].replace(" ","");
                var year = timeStamp.getFullYear() == now.getFullYear() ? "" :  " "+timeStamp.getFullYear();
                return day + " " + month + year;
            }
        }

        
     $(document).ready(function(){

        
    //     ReactDOM.render(<div>
    //<ListArticle  url="http://autonews.dev/api/articles?sourceId=1&limit=10" title="Dân trí" />
    //<ListArticle  url="http://autonews.dev/api/articles?sourceId=2&limit=10" title="Vnexpress" />
    //<ListArticle  url="http://autonews.dev/api/articles?sourceId=3&limit=10" title="Thanh Niên" />
    // <ListArticle  url="http://autonews.dev/api/articles?sourceId=4&limit=10" title="Zing News" />   
    //    </div>
    //, document.getElementById('news-container'));

         if (window.isLogined) {
             ReactDOM.render(
             <SourcesConfig></SourcesConfig>, document.getElementById('sources-config-region'));

         }
         else {
             $("#config-region").hide();
         }

         var hash = window.location.hash;
         $("a[href='" + hash + "']").trigger("click");
         $("#news-container .panel .icon").toggleClass("glyphicon-minus");
         $("#news-container .panel .icon").toggleClass("glyphicon-plus");
         $("#news-container .panel .panel-body").hide();
        

        $("#category-region a").click(function (e) {
            $("#category-region a").removeClass("active");
                $(this).addClass("active");
                //e.preventDefault();
                var categoryId = $(this).attr("categoryId");
                var title = $(this).text();
                var url = "http://autonews.dev/api/categories/"+categoryId+"/articles?limit=10";
                var news = ReactDOM.render(
                      <ListArticle  url={url} title={title} />
                    , document.getElementById('news-container'));
                
                news.componentDidMount();
            });

                $("#search-news-input").keypress(function(e){
                    if(e.keyCode==13){
                        var queryString = $(e.target).val();

                        doSearch(queryString);
                    }
                });
            function doSearch(query){
                var url = "http://autonews.dev/api/articles?searchString="+query;
                var news = ReactDOM.render(
                <ListArticle url={url} title="Kết quả tìm kiếm" />
                , document.getElementById('news-container'));
              
                news.componentDidMount();
            }

    });
