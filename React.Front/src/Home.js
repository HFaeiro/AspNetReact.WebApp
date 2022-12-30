import React, { Component } from 'react';


export class Home extends Component {
    render() {
        //let contents = username if it exists.
        let contents = this.props.profile.username ?
            <h3>Hello, {this.props.profile.username}</h3>
            : <h3>Hello Please Login!</h3>

        return (
            <div className="mt-5 d-flex justify-content-left">

                {contents}

            </div>
        );
    }


}