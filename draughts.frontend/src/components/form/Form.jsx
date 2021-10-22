import React from "react";

import styles from "./Form.module.css";

export default class Form extends React.Component {
	constructor(props) {
		super(props);
		this.children = [];
	}

	render() {
		return (
			<div className={styles.container}>
				<form className={styles.form} onSubmit={(e) => this.onSubmit(e)}>
					{this.renderInputs()}
				</form>
			</div>
		)
	}

	renderInputs() {
		this.children = [];
		return React.Children.map(this.props.children, (child, i) => {
			this.children.push(React.createRef());
			if (React.isValidElement(child) && child.props.field) {
				return React.cloneElement(child, { onRef: (ref) => this.children[ i ] = ref, value: this.props.value[ child.props.field ], onChange: (value) => this.onInputChange(child.props.field, value) });
			}
			return child;
		});
	}

	onInputChange(field, inputValue) {
		var value = this.props.value;
		value[ field ] = inputValue;
		if (this.props.onChange) {
			this.props.onChange(value);
		}
	}

	async onSubmit(e) {
		e.preventDefault();

		if (this.props.onSubmit) {
			this.props.onSubmit(e);
		}
	}
}
