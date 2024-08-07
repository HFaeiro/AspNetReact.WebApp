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
        this.mediaSource = new MediaSource();
        this.init = null;
        this.chunkCount = 0;
        this.totalReceivedChunkTime = 0;
        this.totalExpectedChunkTime = 0;
        /*//replaced by chunkcount by mediaSource update
        this.fetchedVideo =
        {
            id: null,
            video: [],

        };
        */
        this.master =
        {
            GUID: this.props.GUID,
            index:
                [
                    {
                        bandwidth: null,
                        resolution: null,

                    }],
            codex: null,
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

            videoSrc: null,

            token: props.token
        };
    }


    createQuery = async (attributes) => {
        var retQuery = "";

        for (var a in attributes) {

            retQuery += attributes[a].name + '=' + attributes[a].value + '&';
        }
        if (retQuery[retQuery.length - 1] === '&') {
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
            for (var i in Master) 
                {
                    var line = this.props.master[i];
                    if (/GUID/.test(line)) {
                        var keyVal = line.split('=')[1];
                        master.GUID = keyVal;
                        continue;
                    }
                    if (/CODECS/.test(line)) {

                        var c1 = line.split(',')[2];
                        var c2 = line.split(',')[3];
                        var keyVal = c1 + (c2 !== undefined ? ',' + c2 : '');
                        master.codex = keyVal;
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
                        continue;
                    }
                }
            
            this.master.GUID = master.GUID;
            this.master.index = master.indecies;
            this.master.codex = master.codex;
            return master;
        }
    }

    parseIndex = async (Index) => {
        if (Index) {


            var index =
            {
                targetDuration: null,
                playList: [],
                length: 0,
            };
            for (var i in Index) {

                var line = Index[i];
                if (/EXTINF/.test(line)) {
                    var chunkLength = Index[i].split(':')[1].split(',')[0];
                    index.playList.push(chunkLength);
                    index.length += parseFloat(chunkLength);
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
        console.log("Setting up Index :" + (this.master.currentIndex));
        this.index = await this.getIndex(this.master.GUID, this.master.currentIndex);

        //if (index && index.playList && index.playList.length) {
        /* let video = await this.getVideo(this.master.GUID, this.master.currentIndex, this.master.currentTsIndex);
         if (video) {
             this.pushVideoToCache(this.master.GUID, video);
 
          
          const videoElement = document.querySelector("video");
          if(videoElement){
          videoElement.src = video;
          }
          else
          {
                              this.setState(
                 { currentViewingChunk: video }
             )
              }
 
              
 
         }*/
        //}
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
                await this.setupNewIndexAndSetNewChunk();
                this.init = await this.getVideo(this.master.GUID, this.master.currentIndex, -1);
                if (this.init) {
                    this.init = await this.init.arrayBuffer();
                    this.mediaSource.addEventListener('sourceopen', this.sourceopen);



                    this.setState(
                        {
                            videoSrc: URL.createObjectURL(this.mediaSource)
                        });
                }

            }
        }
    }

    updateEnd = async (event) => {
        console.log('updateend');
        this.mediaSource.activeSourceBuffers[0].removeEventListener("updateend", this.updateEnd);
        await this.onTimeUpdate();
        if (this.totalReceivedChunkTime === this.totalExpectedChunkTime) {
            var eos = true;
            for (var i = 0; i < this.mediaSource.activeSourceBuffers.length; i++) {
                if (this.mediaSource.activeSourceBuffers[i].updating) {
                    eos = false;
                    break;
                }
            };
            if (eos) {
                if (this.mediaSource.readyState == 'open') {
                    this.mediaSource.endOfStream();
                }
            }
        }
    }
    onSeek(eTarget) {
        console.log('seek! to :', eTarget.currentTime);
 
    }
    onPlay(eTarget) {

    }
    sourceopen = async (event) => {

        this.master.codex = "video/mp4; " + (this.master.codex ? this.master.codex : 'codecs="avc1.640028, mp4a.40.2"');
        if (MediaSource.isTypeSupported(this.master.codex)) {

            const sourceBuffer = this.mediaSource.addSourceBuffer(this.master.codex);
            sourceBuffer.mode = 'segments';
            sourceBuffer.appendBuffer(this.init);

            sourceBuffer.addEventListener('updateend', this.updateEnd);

            var video = document.getElementsByClassName("VideoPlayer");
            if (video) {

                video[0].addEventListener("seeking", (e) => this.onSeek(e.target));
                video[0].addEventListener("play", (e) => this.onPlay(e.target));
                if (this.totalExpectedChunkTime > 0) {
                    var videoLength = this.totalExpectedChunkTime;
                    if (this.mediaSource.readyState === 'open') {
                        this.mediaSource.setLiveSeekableRange(videoLength, videoLength);
                    }
                }
            }

        } else {
            console.warn(this.master.codex + " not supported");

            this.setState(
                {
                    videoSrc: null
                });
        }

    }
    get = async (query) => {
        return await new Promise(resolve => {
            fetch('/' + process.env.REACT_APP_API + 'stream/' + query, {
                headers: {
                    'Authorization': 'Bearer ' + this.state.token,
                    'Accept': '*/*',
                    'Accept-Encoding': 'gzip, deflate, br'

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
    /*Deprecated by MediaSource
    addVideoToCacheAt = async (guid, video, index) => {

        this.fetchedVideo.video[index] = video;
        this.fetchedVideo.id = this.master.GUID;

    }

    pushVideoToCache = async (guid, video) => {
        this.fetchedVideo.video.push(video);
        this.fetchedVideo.id = this.master.GUID;
    }
    */
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
                    //var video = URL.createObjectURL(data);
                    if (DataIndex >= 0)
                        this.chunkCount++;
                    return data;
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
                this.parseIndex(data);
                if (this.indecies.index[0] && this.indecies.index[0].length) {
                    this.totalExpectedChunkTime = this.indecies.index[0].length;
                }
            })
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
        /* if (this.vRef.current.currentTime === 0 && this.master.currentTsIndex != 0) {
             this.vRef.current.play();
             return;
         }*/

        if ((this.vRef.current.duration == NaN || this.vRef.current.duration == undefined) &&
            (this.vRef.current.currentTime >= (this.vRef.current.duration) * .5)) {
            return;

        }
        let currentChunkTime = 0;
        var video = null;
        if (this.indecies.index[this.master.currentIndex]) {
            currentChunkTime = parseFloat(this.indecies.index[this.master.currentIndex].playList[this.chunkCount])
        }
        else {
            return setTimeout(this.onTimeUpdate, 2600);
        }
        if (this.totalReceivedChunkTime !== 0 && this.vRef.current.currentTime >= this.totalReceivedChunkTime - (currentChunkTime * .2)) {

            /*deprecated by mediaSource
            if (!this.fetchedVideo.video[this.master.currentTsIndex + 1] || this.fetchedVideo.video[this.master.currentTsIndex + 1] == undefined || this.fetchedVideo.video[this.master.currentTsIndex + 1] == NaN) {
           */

            if (this.chunkCount <= this.master.currentTsIndex) {

                if (this.indecies.index[this.master.currentIndex].playList.length > this.master.currentTsIndex + 1) {
                    if (!this.frameRequestLock) {
                        this.frameRequestLock = true;
                        this.master.currentTsIndex++;
                        console.log("Requesting and switching to next frame #" + (this.master.currentTsIndex));
                        video = await this.getVideo(this.master.GUID, this.master.currentIndex, this.master.currentTsIndex);

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
                    //not sure I need this anymore with the media source stuff?
                    /*this.setState(
                        { currentViewingChunk: this.fetchedVideo.video[this.master.currentTsIndex] }
                    );*/

                    this.master.currentTsIndex = 0;
                    console.log("Reseting Video index to #" + (this.master.currentTsIndex));
                }


            }
            else {

                this.master.currentTsIndex++;
                this.timeOfLastFrameRequest = this.timeOfLastFrameRequest - currentChunkTime;
                console.log("Switching to new Frame #" + (this.master.currentTsIndex + 1));
                this.frameLockCount--;

                /* //not sure I need this anymore with the media source stuff?
                const videoElement = document.querySelector("video");
                if (videoElement) {
                    videoElement.src = this.fetchedVideo.video[this.master.currentTsIndex];
                }
                */

                // this.vRef.srcObject = this.fetchedVideo.video[this.master.currentTsIndex];
            }
        }


        else {
            if (!this.indecies || !this.indecies.index || this.indecies.index.length < 1 || !this.indecies.index[this.master.currentIndex].playList) {
                return setTimeout(this.onTimeUpdate, 2600);
            }
            if (this.indecies.index[this.master.currentIndex].playList.length > this.chunkCount) {

                if (this.vRef.current.currentTime - this.timeOfLastFrameRequest > .5 && !this.frameRequestLock || this.timeOfLastFrameRequest == 0) {
                    this.frameRequestLock = true;
                    this.timeOfLastFrameRequest = this.vRef.current.currentTime;


                    video = await this.getVideo(this.master.GUID, this.master.currentIndex, this.chunkCount)
                    if (video) {
                        if (this.frameRequestLock) {
                            //this.pushVideoToCache(this.master.GUID, video)
                            console.log("Received Frame #" + (this.chunkCount));
                        }
                        else {
                            console.log("Received Frame #" + (this.chunkCount) + " but frame lock was over written. No longer safe to push this video to buffer. ");
                            video = null;
                        }
                    }
                    this.frameRequestLock = false;
                }
                /*
                else {
                    //try again? 
                    return setTimeout(this.onTimeUpdate, 2600);
                }
                */
            }
            if (video) {
                if (this.mediaSource.sourceBuffers.length) {
                    this.totalReceivedChunkTime += currentChunkTime;
                    this.mediaSource.sourceBuffers[0].timestampOffset = this.master.currentIndex ? currentChunkTime : 0;
                    let buffer = await video.arrayBuffer();
                    await this.mediaSource.sourceBuffers[0].appendBuffer(buffer);
                    this.mediaSource.sourceBuffers[0].addEventListener('updateend', this.updateEnd);

                }
            }
            return video;
        }
    }



    render() {

        //if (this.state.currentViewingChunk) {
        //     console.log(this.state.currentViewingChunk);
        // }
        var content = this.state.videoSrc ?
            <>
                <video className="VideoPlayer" controls muted ref={this.vRef} onTimeUpdate={this.onTimeUpdate}
                    src={this.state.videoSrc} >
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