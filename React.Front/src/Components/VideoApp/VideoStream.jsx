import React, { Component } from 'react';
import { Table } from 'react-bootstrap'
import { Navigate, useParams } from 'react-router-dom';
import { withRouter } from '../../Utils/withRouter';
import { Helmet } from "react-helmet";
import './Video.css';

export class VideoStream extends Component {
    constructor(props) {
        super(props);
        this.state =
        {
            master:
            {
                GUID: null,
                index:
                    [
                        {
                            bandwidth: null,
                            resolution: null,

                        }],

            },
            index:
            {
                targetDuration: null,
                playList: [],
            },
            fetchedVideo:
            {
                id: null,
                video: null
            },
            token: props.token
        }
    }

    createQuery = async (query) => {
        var retQuery = "";
        for (var q in query) {

            retQuery += query[q] + '&';
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
            var index =
            {
                targetDuration: null,
                playList: [],
            };
            for (var i in Index) {

                var line = Index[i];
                if (/EXTINF/.test(line)) {
                    index.playList.push(Index[++i]);
                    console.log("Added to play list : " + Index[i]);
                    continue;
                }
                if (/X-END/.test(line)) {
                    break;
                }
                if (/X-TA/.test(line)) {
                    var keyVal = line.split(':')[1];
                    index.targetDuration = keyVal;
                    continue;
                }

            }
            this.setState({
                index: index
            })
            return index;
        }
    }

    async componentDidMount() {
        if (this.state.fetchedVideo?.video == null) {
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
                    console.log(data);

                    resolve(data);
                })
        })
    }
        

    

    getVideo = async (GUID, Index, DataIndex) => {
        var query = await this.createQuery({ GUID, Index, DataIndex });
        return await this.get("data?guid=" + query)
            .then(data => {

                var video = URL.createObjectURL(data);

                this.setState(
                    ({
                        fetchedVideo:
                        {
                            id: GUID,
                            video: video
                        }
                    }));
                return video;
            })

    }
    getIndex = async (GUID, Index) => {
        var query = await this.createQuery({ GUID, Index});
        return await this.get("index?guid=" + query)
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

    render() {


        var content = this.state.fetchedVideo && this.state.fetchedVideo.video ? 
            <>
                <video className="VideoPlayer" controls muted
                    src={this.state.fetchedVideo.video} >
                </video>

            </> :
            <>

            </>

        return (<>
            {content }


        </>);
    }

    
}