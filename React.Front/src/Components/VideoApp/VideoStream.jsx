import React, { useRef, Component } from 'react';
import './Video.css';

export class VideoStream extends Component {
    constructor(props) {
        super(props);
        this.onTimeUpdate = this.onTimeUpdate.bind(this);
        this.vRef = React.createRef();
        this.frameRequestLock = false;
        this.timeSinceLastFrameRequest = 0;
        this.timeOfThisFrameRequest = 0;
        this.timeSinceLastFrameRequest = 0;
        this.frameLockCount = new Map();
        this.mediaSource = new MediaSource();
        this.init = null;
        this.totalReceivedChunkTime = 0;
        this.totalExpectedChunkTime = 0;
        this.receivedChunksArray = new Map();
        this.seeked = false;
        this.collectedChunksAfterCurrent = 0;
        this.lastIndex = 0;
        this.seekAheadLimit = 8;
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
            for (var i in Master) {
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
        //console.log('updateend');
        let activeSource = this.mediaSource.activeSourceBuffers[0];
        if (activeSource) {
            activeSource.removeEventListener("updateend", this.updateEnd);
        }
        if (this.master.currentTsIndex < 1) {
            await this.onTimeUpdate();
        }
        if (this.totalReceivedChunkTime === this.totalExpectedChunkTime || this.master.currentTsIndex >= this.indecies.index[this.master.currentIndex]?.playList.length) {
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
    async onSeek(eTarget) {
        let video = null;
        //if (eTarget.currentTime > this.totalReceivedChunkTime) {
        let chunkTime = 0;
        if (this.indecies.index[this.master.currentIndex]) {
            chunkTime = parseFloat(this.indecies.index[this.master.currentIndex].playList[Math.round(0)])
            if (chunkTime > 0) {
                let estimatedRequestedIndex = Math.round(eTarget.currentTime / chunkTime);
                if (estimatedRequestedIndex != this.master.currentTsIndex) {
                    this.master.currentTsIndex = estimatedRequestedIndex - 1;
                    console.log("We Seekith a new Chunk! #", this.master.currentTsIndex);
                    this.seeked = true;
                    this.onTimeUpdate();
                    return;
                }
            }
        }
        this.seeked = true;
    }

    onPlay(eTarget) {

    }

    sourceopen = async (event) => {

        this.master.codex = "video/mp4; " + (this.master.codex ? this.master.codex : 'codecs="avc1.640028, mp4a.40.2"');
        if (MediaSource.isTypeSupported(this.master.codex)) {
            let sourceBuffer = null;
            if (!this.mediaSource.activeSourceBuffers || !(this.mediaSource.activeSourceBuffers.length > 0)) {
                sourceBuffer = this.mediaSource.addSourceBuffer(this.master.codex);
            }
            else {
                sourceBuffer = this.mediaSource.activeSourceBuffers[0];
            }
            if (sourceBuffer == null) {
                return;
            }
            if (sourceBuffer.updating == false) {
                sourceBuffer.mode = 'segments';
                sourceBuffer.appendBuffer(this.init);

                sourceBuffer.addEventListener('updateend', this.updateEnd);

                var video = document.getElementsByClassName("VideoPlayer");
                if (video) {

                    video[0].addEventListener("seeking", (e) => this.onSeek(e.target));
                    video[0].addEventListener("seeked", (e) => this.onSeek(e.target));
                    if (this.totalExpectedChunkTime > 0) {
                        var videoLength = this.totalExpectedChunkTime;
                        if (this.mediaSource.readyState === 'open') {
                            this.mediaSource.setLiveSeekableRange(videoLength, videoLength);
                        }
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

    getVideo = async (GUID, Index, DataIndex) => {
        console.log("Requesting New Chunk #" + (DataIndex));
        let query = [

            { name: "guid", value: GUID },
            { name: "index", value: Index },
            { name: "dataIndex", value: DataIndex }
        ]

        query = await this.createQuery(query);
        return await this.get("data?" + query)
            .then(data => {
                if (data) {
                    return data;
                }
            })
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
    getChunkTime() {
        let currentChunkTime = 0;
        if (this.indecies.index[this.master.currentIndex]) {
            currentChunkTime = parseFloat(this.indecies.index[this.master.currentIndex].playList[this.master.currentTsIndex])
        }
        return currentChunkTime;
    }
    onTimeUpdate = async () => {
        if (!this.vRef || !this.vRef.current) {
            return;
        }

        if ((this.vRef.current.duration == NaN || this.vRef.current.duration == undefined) &&
            (this.vRef.current.currentTime >= (this.vRef.current.duration) * .5)) {
            return;

        }
        let currentChunkTime = this.getChunkTime();
        var video = null;
        if (currentChunkTime == 0) {
            return setTimeout(this.onTimeUpdate, 2600);
        }
        this.timeSinceLastFrameRequest = this.vRef.current.currentTime - this.timeOfThisFrameRequest;
        this.timeOfThisFrameRequest = this.vRef.current.currentTime;

        let estimatedCurrentIndex = Math.round(this.vRef.current.currentTime / currentChunkTime);
        if (this.vRef.current.currentTime < currentChunkTime) {
            estimatedCurrentIndex = 0;
        }
        if (this.seeked == true || !this.collectedChunksAfterCurrent) {

            let tmpChunkIndex = estimatedCurrentIndex;
            this.collectedChunksAfterCurrent = 0;
            this.receivedChunksArray.forEach(function(chunkvalue, chunk) {
                if (chunk == tmpChunkIndex) {
                    tmpChunkIndex++;
                    this.collectedChunksAfterCurrent++;
                    if (tmpChunkIndex > estimatedCurrentIndex + this.seekAheadLimit) {
                        return;
                    }
                }
            }.bind(this));
        }

        if (this.lastIndex != estimatedCurrentIndex) {
            let newCollectedCount = this.collectedChunksAfterCurrent - (estimatedCurrentIndex - this.lastIndex);
            this.collectedChunksAfterCurrent = newCollectedCount > 0 ? newCollectedCount : 0;
        }

        this.lastIndex = estimatedCurrentIndex;

        this.receivedChunkTimeOffset = currentChunkTime * this.collectedChunksAfterCurrent;
        let haveChunk = this.receivedChunksArray.get(this.master.currentTsIndex);
        if (haveChunk == 1) {
            console.log("Already Have chunk : ", this.master.currentTsIndex);
            this.collectedChunksAfterCurrent++;
            this.master.currentTsIndex++;
            return;
        }
        if ((this.collectedChunksAfterCurrent > this.seekAheadLimit || (this.master.currentTsIndex - estimatedCurrentIndex > this.seekAheadLimit)) && !this.seeked) {
            console.log("got too many chunks! returning, estimatedCurrentIndex:", estimatedCurrentIndex, " chunks ahead:", this.collectedChunksAfterCurrent)
            setTimeout(currentChunkTime * 1000);
            return;
        }




        if ((this.totalReceivedChunkTime !== 0 && (this.receivedChunkTimeOffset <= currentChunkTime || this.timeSinceLastFrameRequest > 1)) && !this.seeked) {
            if (this.indecies.index[this.master.currentIndex].playList.length > this.master.currentTsIndex) {
                if (!this.frameRequestLock && (this.timeSinceLastFrameRequest > .0025 || this.timeSinceLastFrameRequest == 0)) {
                    this.frameRequestLock = true;
                    console.log("Requesting and switching to next Chunk #" + (this.master.currentTsIndex));
                    video = await this.getChunk();
                    if (video) {
                        await this.appendBuffer(video, currentChunkTime);
                        this.collectedChunksAfterCurrent++;
                        this.master.currentTsIndex++;
                        this.frameRequestLock = false;
                        this.frameLockCount.set(this.master.currentTsIndex, 0);
                    }
                    else {
                        this.frameRequestLock = false;
                        this.frameLockCount.set(this.master.currentTsIndex, 0);
                    }
                    return this.onTimeUpdate();
                }
                else if (this.timeSinceLastFrameRequest > 1) {
                    let currentFrameLock = this.getFrameLockCount(this.master.currentTsIndex);
                    if (currentFrameLock < 4) {
                        console.log("Frame Locked Trying For Chunk " + (this.master.currentTsIndex) + " Again ");
                        currentFrameLock++
                        this.frameLockCount.set(this.master.currentTsIndex, currentFrameLock);
                        return setTimeout(this.onTimeUpdate, 2600);
                    }
                    else {
                        console.log("Attempted Chunk " + (this.master.currentTsIndex) + " Too Many Times Checking for new Index");
                        if (this.master.currentIndex + 1 < this.master.index.length) {
                            this.frameRequestLock = false;
                            this.frameLockCount.set(this.master.currentTsIndex, 0);
                            this.master.currentIndex++;
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
                return setTimeout(this.onTimeUpdate, 2600);
            }
        }
        else {
            if (!this.indecies || !this.indecies.index || this.indecies.index.length < 1 || !this.indecies.index[this.master.currentIndex].playList) {
                return setTimeout(this.onTimeUpdate, 2600);
            }
            
            if (this.indecies.index[this.master.currentIndex].playList.length > this.master.currentTsIndex) {

                if (!this.frameRequestLock && (this.timeSinceLastFrameRequest > .05 || this.timeSinceLastFrameRequest == 0)) {
                    this.frameRequestLock = true;
                    video = video = await this.getChunk();

                    if (!video) {
                        this.frameRequestLock = false;
                        return this.onTimeUpdate();
                    }
                    this.frameRequestLock = false;
                }
            }
            if (video) {
                await this.appendBuffer(video, currentChunkTime);
                this.collectedChunksAfterCurrent++;
                this.master.currentTsIndex++;
                if (this.seeked) {
                    this.seeked = false;
                    return this.onTimeUpdate();
                }
            }

        }

    }

    getFrameLockCount(chunkNumber) {
        let currentFrameLock = this.frameLockCount.get(chunkNumber);
        if (currentFrameLock == undefined) {
            currentFrameLock = 0;
        }
        return currentFrameLock;
    }
    async getChunk() {
        let requestedChunk = this.master.currentTsIndex;
        let video = await this.getVideo(this.master.GUID, this.master.currentIndex, requestedChunk)
        if (video) {
            if (this.frameRequestLock && requestedChunk === this.master.currentTsIndex) {
                //onsole.log("Received Chunk #" + (this.master.currentTsIndex));
                if (requestedChunk) {
                    this.receivedChunksArray.set(requestedChunk, 1);
                    //console.log("pushed chunk #", requestedChunk);
                }
            }
            else {
                console.log("Received Chunk #" + (requestedChunk) + " No longer safe to push this video to buffer. ");
                return null;
            }
        }
        return video;
    }
    async appendBuffer(video, chunkTime) {
        if (this.mediaSource.sourceBuffers.length) {
            this.totalReceivedChunkTime += chunkTime;
            //this.mediaSource.sourceBuffers[0].timestampOffset = this.master.currentIndex ? chunkTime : 0;
            let buffer = await video.arrayBuffer();
            //console.log("appending buffer!");
            await this.mediaSource.sourceBuffers[0].appendBuffer(buffer);
            this.mediaSource.sourceBuffers[0].addEventListener('updateend', this.updateEnd);
        }
    }


    render() {
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