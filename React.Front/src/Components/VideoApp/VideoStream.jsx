import React, { useRef, Component } from 'react';
import { Table } from 'react-bootstrap'
import { Navigate, useParams } from 'react-router-dom';
import { withRouter } from '../../Utils/withRouter';
import { Helmet } from "react-helmet";
import './Video.css';

export class VideoStream extends Component {
    constructor(props) {
        super(props);
        this.onTimeUpdate = this.onTimeUpdate.bind(this);
        this.vRef = React.createRef();
        this.frameRequestLock = false;
        var index:
            {
                targetDuration: null,
                playList: [],
            };
        this.state =
        {

            master:
            {
                GUID: this.props.GUID,
                index:
                    [
                        {
                            bandwidth: null,
                            resolution: null,

                        }],
                currentIndex: null,
                currentTsIndex: null,
            },
            indecies:
                [{
                    index,
                }],

            fetchedVideo:
            {
                id: null,
                video: [],
                currentFetched: 0
            },

            token: props.token
        };
    }


    createQuery = async (attributes) => {
        var retQuery = "";

        for (var a in attributes) {

            retQuery += attributes[a].name + '=' + attributes[a].value + '&';
        }
        if (retQuery[retQuery.length - 1] == '&') {
            retQuery = retQuery.substr(0, retQuery.length - 1);
        }
        return retQuery;
    }


    parseMaster = async (Master) => {
        if (Master) {
            var master =
            {
                GUID: null,
                indecies: [],
            };
            for (var i in Master) {
                {
                    var line = this.props.master[i];
                    if (/GUID/.test(line)) {
                        var keyVal = line.split('=')[1];
                        master.GUID = keyVal;
                        continue;
                    }
                    if (/STREAM-INF/.test(line)) {
                        var keyVal = line.split(':')[1];
                        if (/BANDWIDTH/.test(keyVal)) {
                            var bandwidth = keyVal.split('=')[1].split(',')[0],
                                resolution = keyVal.split(',')[1].split(',')[0].split('=')[1];

                            master.indecies.push({ bandwidth, resolution });

                            console.log("Resolution Found : " + resolution);
                        }
                    }
                }
            }
            this.setState({
                master: master
            })
            return master;
        }
    }

    parseIndex = async (Index) => {
        if (Index) {

            var indecies =
                [

                ],
                index =
                {
                    targetDuration: null,
                    playList: [],
                };
            for (var i in Index) {

                var line = Index[i];
                if (/EXTINF/.test(line)) {
                    index.playList.push(Index[++i]);
                    //console.log("Added to play list : " + Index[i]);
                    continue;
                }
                if (/X-END/.test(line)) {
                    break;
                }
                if (/X-TA/.test(line)) {
                    var keyVal = line.split(':')[1];
                    index.targetDuration = keyVal;
                    indecies.push(index);
                    continue;

                }

            }
            let master = this.state.master;
            master.currentIndex = 0;
            master.currentTsIndex = 0;
            this.setState({
                indecies: indecies,
                master: master
            });
            console.log("Added playlists : " + indecies[0].playList.length);

            return index;
        }
    }

    async componentDidMount() {
        if (!this.state.fetchedVideo.video[this.state.master.currentTsIndex + 1] || this.state.fetchedVideo.video[this.state.master.currentTsIndex + 1] == undefined || this.state.fetchedVideo.video[this.state.master.currentTsIndex + 1] == NaN) {
            let master = await this.parseMaster(this.props.master)
            if (master && master.GUID) {
                var index = await this.getIndex(master.GUID, 0);
                if (index && index.playList && index.playList.length) {
                    this.getVideo(master.GUID, 0, 0);
                }
            }
        }

    }

    get = async (query) => {
        return await new Promise(resolve => {
            fetch('/' + process.env.REACT_APP_API + 'stream/' + query, {
                headers: {
                    'Authorization': 'Bearer ' + this.state.token,
                    'Accept': '*/*',
                    'Accept-Encoding': 'gzip, deflate, br',
                    'Connection': 'keep-alive'
                }

            })
                .then(res => {
                    if (res.status == 201) {
                        return (res.blob());
                    }
                    else if (res.status == 202) {
                        return (res.text());
                    }
                    else
                        return;

                }).then(data => {
                    //console.log(data);

                    resolve(data);
                })
        })
    }




    getVideo = async (GUID, Index, DataIndex) => {

        if (!this.frameRequestLock) {
            let query = [

                { name: "guid", value: GUID },
                { name: "index", value: Index },
                { name: "dataIndex", value: DataIndex }
            ]

            query = await this.createQuery(query);
            return await this.get("data?" + query)
                .then(data => {
                    if (data) {
                        var video = URL.createObjectURL(data);
                        let fetchedVideo = this.state.fetchedVideo;
                        fetchedVideo.video.push(video);
                        fetchedVideo.id = GUID;
                        if(fetchedVideo.currentFetched == 0)
                            fetchedVideo.currentFetched = 1;
                        this.setState(
                            ({
                                fetchedVideo: fetchedVideo
                            }));
                    }
                    this.frameRequestLock = false;
                    return video;
                })
            this.frameRequestLock = false;
            return null;
        }
        return null;
    }
    getIndex = async (GUID, Index) => {

        let query = [
            { name: "guid", value: GUID },
            { name: "index", value: Index }
        ]

        query = await this.createQuery(query);
        return await this.get("index?" + query)
            .then(data => {
                if (data) {
                    return data.split(/[\r\n]/);
                }
            })
            .then(data => {
                return this.parseIndex(data);
            })
        return;
    }
    onTimeUpdate = async () => {
        if (!this.vRef || !this.vRef.current) {
            return;
        }
        if (this.vRef.current.currentTime === 0 && this.state.master.currentTsIndex != 0) {
            this.vRef.current.play();
            return;
        }

        if ((this.vRef.current.duration == NaN || this.vRef.current.duration == undefined) &&
            (this.vRef.current.currentTime >= (this.vRef.current.duration) * .5)) {
            return;

        }

        if (this.vRef.current.currentTime === this.vRef.current.duration) {
            
             if(!this.state.fetchedVideo.video[this.state.master.currentTsIndex + 1] || this.state.fetchedVideo.video[this.state.master.currentTsIndex+1] == undefined || this.state.fetchedVideo.video[this.state.master.currentTsIndex+1] == NaN)
                {
                    let uno = 1;
                  
                    if(this.state.indecies[this.state.master.currentIndex].playList.length > this.state.master.currentTsIndex + 1)                   
                    {                                              
                        let master = this.state.master;                                       
                        master.currentTsIndex++;                                       
                        this.setState({                               
                            master: master                                     
                        });                 
                    
                        this.getVideo(master.GUID,master.currentIndex,master.currentTsIndex);                                             
                        console.log("Requesting and switching to next frame #" + (master.currentTsIndex) );                            
                    }                           
                    else                   
                    {                                                 
                         let master = this.state.master;                                    
                         master.currentTsIndex = 0;                                                      
                         this.setState({                                                                 
                             master: master                                                          
                         });  
                         console.log("Reseting index to #" + (master.currentTsIndex) );                 
                    }                                            
              }
               else
                {
                      let master = this.state.master;            
                   
                    
                         master.currentTsIndex++;
                    
                    
                         this.setState({                               
                           
                             master: master                           
                        
                         });
                         console.log("Switching to new Frame #" + (master.currentTsIndex) );
                }
        }


        else {

            if (this.state.indecies[this.state.master.currentIndex].playList.length > this.state.fetchedVideo.currentFetched) {

                console.log("Requesting New Frame #" + (this.state.fetchedVideo.currentFetched));
                if(this.getVideo(this.state.master.GUID, this.state.master.currentIndex, this.state.fetchedVideo.currentFetched))
                {

                    let fetchedVideo = this.state.fetchedVideo;            
                   
                    
                        fetchedVideo.currentFetched++;
                    
                    
                         this.setState({                               
                           
                            fetchedVideo: fetchedVideo                           
                        
                         });
                }
                
            }
        }







    }
    render() {


        var content = this.state.fetchedVideo && this.state.fetchedVideo.video[this.state.master.currentTsIndex] ?
            <>
                <video className="VideoPlayer" controls muted ref={this.vRef} duration={50} onTimeUpdate={this.onTimeUpdate}
                    src={this.state.fetchedVideo.video[this.state.master.currentTsIndex]} >
                </video>

            </> :
            <>

            </>

        return (<>
            {content}


        </>);
    }


}