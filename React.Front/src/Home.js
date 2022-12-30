import React, { Component } from 'react';


export class Home extends Component {
    state =
        {
            defaultUser:[]
            }

    async handleSubmit() {
        fetch(process.env.REACT_APP_API + 'users/' + '1', {
            method: 'Get',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }

        }).then(res => res.json())
                .then((result) => {
                    this.setState({ defaultUser: result });


    },
        (error) => {
            alert('Failed:' + error);
        })

    }

    componentDidMount() {
        this.handleSubmit();
    }

    render() {
        //let contents = username if it exists.
        let contents = this.props.profile.username ?
            <h3>Hello, {this.props.profile.username}</h3>
            : <div><h3>Hello stranger!</h3>
                <div><span>
                    You can either create a user or you can login using the default!
                </span>
                </div>
                <span>
                    Try this Username : {this.state.defaultUser.username}
                </span>
                <div> <span>
                   And this Password! : {this.state.defaultUser.password}
                </span>
                </div>
                <div> <span>
                    Might see the password change right here if you edit it 
                </span>
                </div>
            </div>
        return (
            <div className="mt-5 d-flex justify-content-left">

                {contents}

            </div>
        );
    }


}