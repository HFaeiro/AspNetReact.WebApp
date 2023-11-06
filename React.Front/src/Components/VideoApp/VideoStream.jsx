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
        this.timeSinceLastFrameRequest = 0;
        this.timeOfLastFrameRequest = 0;
        this.frameLockCount = 0;
        this.fetchedVideo =
        {
            id: null,
            video: [],

        };

        this.master =
        {
            GUID: this.props.GUID,
            index:
                [
                    {
                        bandwidth: null,
                        resolution: null,

                    }],
            currentIndex: 0,
            currentTsIndex: 0,
        };
        this.indecies =
        {
            index:
                [

                ]
        };

        this.state =
        {

            loading: false,

            currentViewingChunk: null,

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
            this.master.GUID = master.GUID;
            this.master.index = master.indecies;
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
                    //console.log("Added to play list : " + Index[i]);
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

            this.indecies.index.push(index);


            console.log("Added playlists : " + index.playList.length);

            return index;
        }
    }
    setupNewIndexAndSetNewChunk = async () => {
        var index = await this.getIndex(this.master.GUID, this.master.currentIndex);
        if (index && index.playList && index.playList.length) {
            let video = await this.getVideo(this.master.GUID, this.master.currentIndex, this.master.currentTsIndex);
            if (video) {
                this.pushVideoToCache(this.master.GUID, video);
                this.setState(
                    { currentViewingChunk: video }
                )
            }
        }
    }
    async componentDidMount() {
        if (!this.state.currentViewingChunk) {
            let master = await this.parseMaster(this.props.master)
            if (master && master.GUID) {
                this.setState(
                    {
                        loading: true
                    }
                )
                this.setupNewIndexAndSetNewChunk();
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

    addVideoToCacheAt = async (guid, video, index) => {

        this.fetchedVideo.video[index] = video;
        this.fetchedVideo.id = this.master.GUID;

    }

    pushVideoToCache = async (guid, video) => {
        this.fetchedVideo.video.push(video);
        this.fetchedVideo.id = this.master.GUID;
    }

    getVideo = async (GUID, Index, DataIndex) => {
        console.log("Requesting New Frame #" + (DataIndex));
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
                    return video;
                }
            })
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
    onLoadStart = async () => {
        if (!this.vRef || !this.vRef.current) {
            return;
        }


    }
    onTimeUpdate = async () => {
        if (!this.vRef || !this.vRef.current) {
            return;
        }
        if (this.vRef.current.currentTime === 0 && this.master.currentTsIndex != 0) {
            this.vRef.current.play();
            return;
        }

        if ((this.vRef.current.duration == NaN || this.vRef.current.duration == undefined) &&
            (this.vRef.current.currentTime >= (this.vRef.current.duration) * .5)) {
            return;

        }

        if (this.vRef.current.currentTime === this.vRef.current.duration) {

            if (!this.fetchedVideo.video[this.master.currentTsIndex + 1] || this.fetchedVideo.video[this.master.currentTsIndex + 1] == undefined || this.fetchedVideo.video[this.master.currentTsIndex + 1] == NaN) {

                if (this.indecies.index[this.master.currentIndex].playList.length > this.master.currentTsIndex + 1) {
                    if (!this.frameRequestLock) {
                        this.frameRequestLock = true;
                        this.master.currentTsIndex++;
                        console.log("Requesting and switching to next frame #" + (this.master.currentTsIndex));
                        let video = await this.getVideo(this.master.GUID, this.master.currentIndex, this.master.currentTsIndex);
                        if (video) {
                            this.addVideoToCacheAt(this.master.GUID, video, this.master.currentTsIndex);
                        }
                        this.frameRequestLock = false;
                    }
                    else {
                        if (this.frameLockCount < 1) {
                            console.log("Frame Locked Trying For Frame " + (this.master.currentTsIndex + 1) + " Again ");
                            this.frameLockCount++;
                            return setTimeout(this.onTimeUpdate, 2600);
                        }
                        else {
                            console.log("Attempted Frame " + (this.master.currentTsIndex + 1) + " Too Many Times Checking for new Index");
                            if (this.master.currentIndex + 1 < this.master.index.length) {
                                this.master.currentIndex++;
                                //this.fetchedVideo.video = null;
                                //this.master.currentTsIndex = 0;
                                //this.setState(
                                //    { currentViewingChunk: null }
                                //);
                                this.master.currentTsIndex++;
                                this.frameLockCount = 0;
                                this.frameRequestLock = false;
                                this.setupNewIndexAndSetNewChunk();
                            }
                            else {

                                console.log("cannot lower quality any further will wait for frame.");
                                return setTimeout(this.onTimeUpdate, 2600);
                            }

                        }
                    }
                }
                else {
                    this.setState(
                        { currentViewingChunk: this.fetchedVideo.video[this.master.currentTsIndex] }
                    );

                    this.master.currentTsIndex = 0;
                    console.log("Reseting Video index to #" + (this.master.currentTsIndex));
                }


            }
            else {

                this.master.currentTsIndex++;
                this.timeOfLastFrameRequest = this.timeOfLastFrameRequest - this.vRef.current.currentTime
                console.log("Switching to new Frame #" + (this.master.currentTsIndex + 1));
                this.frameLockCount--;
                this.setState(
                    { currentViewingChunk: this.fetchedVideo.video[this.master.currentTsIndex] }
                )
            }
        }


        else {

            if (this.indecies.index[this.master.currentIndex].playList.length > this.fetchedVideo.video.length) {

                if (this.vRef.current.currentTime - this.timeOfLastFrameRequest > .5 && !this.frameRequestLock) {
                    this.frameRequestLock = true;
                    this.timeOfLastFrameRequest = this.vRef.current.currentTime;


                    let video = await this.getVideo(this.master.GUID, this.master.currentIndex, this.fetchedVideo.video.length)
                    if (video) {
                        if (this.frameRequestLock) {
                            this.pushVideoToCache(this.master.GUID, video)
                            console.log("Received Frame #" + (this.fetchedVideo.video.length - 1));
                        }
                        else {
                            console.log("Received Frame #" + (this.fetchedVideo.video.length) + " but frame lock was over written. No longer safe to push this video to buffer. ");
                        }
                    }
                    this.frameRequestLock = false;
                }
            }

        }
    }



    render() {

        //if (this.state.currentViewingChunk) {
        //     console.log(this.state.currentViewingChunk);
        // }
        var content = this.state.currentViewingChunk ?
            <>
                <video className="VideoPlayer" controls muted ref={this.vRef} onTimeUpdate={this.onTimeUpdate}
                    src={this.fetchedVideo.video[this.master.currentTsIndex]} >
                </video>

            </> : this.state.loading ?
                <>
                    <p>
                        We found your Video and are loading it! Please be Patient!
                    </p>
                </> :
                <>

                </>

        return (<>
            {content}


        </>);
    }


}